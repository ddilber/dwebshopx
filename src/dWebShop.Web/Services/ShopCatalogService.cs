using dWebShop.Application.Features.Brands.Queries;
using dWebShop.Application.Features.Categories.Queries;
using dWebShop.Application.Features.Products.Queries;
using dWebShop.Application.Services;
using dWebShop.Domain.Entities.Products;
using MediatR;

namespace dWebShop.Web.Services;

// Single read-model facade for the public shop. Wraps MediatR queries and
// IPricingService so razor pages don't depend on Application DTOs directly —
// they get the ShopProductVm / ShopProductDetailVm shape the markup already
// understands.
//
// The legacy ShopData provided three top-level collections — Brands,
// Categories (keyed by brand slug) and Products. This service exposes the
// same trio plus a PDP lookup, with optional partner-aware pricing applied
// inline so the page doesn't have to overlay prices itself.
public class ShopCatalogService(IMediator mediator, IPricingService pricing)
{
    // Brands are returned in display order (Name asc) — same as the upstream
    // GetBrandsQuery which already caches for 5 minutes.
    public async Task<IReadOnlyList<ShopBrandVm>> GetBrandsAsync(CancellationToken ct = default)
    {
        var brands = await mediator.Send(new GetBrandsQuery(), ct);
        return brands
            .Select(b => new ShopBrandVm(
                b.Id, b.Slug, b.Name,
                // Description doubles as the marketing tagline on listing pages.
                Tagline: b.Description,
                // Origin / Since are not on BrandDto today — fall back to empty
                // until the Brand domain entity gains those fields. Pages
                // already cope with empty strings here.
                Origin: string.Empty,
                Since: string.Empty))
            .ToList();
    }

    // Categories keyed by brand slug. Each list contains ROOT categories with
    // their sub-categories nested under Children — the shape mirrors the
    // self-referential Category entity. Pages flatten with Flatten() for
    // display, or recurse if they need nested rendering.
    public async Task<IReadOnlyDictionary<string, IReadOnlyList<ShopCategoryVm>>> GetCategoriesByBrandAsync(CancellationToken ct = default)
    {
        var brands = await mediator.Send(new GetBrandsQuery(), ct);
        var brandSlugById = brands.ToDictionary(b => b.Id, b => b.Slug);

        var categories = await mediator.Send(new GetCategoriesQuery(BrandId: null), ct);
        var result = new Dictionary<string, IReadOnlyList<ShopCategoryVm>>();

        foreach (var grp in categories
            .Where(c => c.BrandId.HasValue && brandSlugById.ContainsKey(c.BrandId.Value))
            .GroupBy(c => brandSlugById[c.BrandId!.Value]))
        {
            result[grp.Key] = BuildCategoryTree(grp.ToList());
        }

        return result;
    }

    // Two-pass tree build: index every dto by id, then walk parent ids to
    // assemble Children lists. Handles arbitrary depth and orphans (categories
    // whose parent id doesn't resolve to any category in this brand are
    // treated as roots, so misconfigured data still renders).
    private static IReadOnlyList<ShopCategoryVm> BuildCategoryTree(List<CategoryDto> flat)
    {
        // Children keyed by parent id. Roots (null parent) are picked up via
        // the separate `roots` filter below; null doesn't satisfy ToDictionary's
        // notnull TKey constraint anyway.
        var byParent = flat
            .Where(c => c.ParentCategoryId.HasValue)
            .GroupBy(c => c.ParentCategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.Name).ToList());

        // All ids that exist as a category in this brand — anything else
        // pointing at a parent id we don't have gets bumped to root.
        var knownIds = flat.Select(c => c.Id).ToHashSet();

        ShopCategoryVm Build(CategoryDto dto)
        {
            var children = byParent.TryGetValue(dto.Id, out var ch)
                ? (IReadOnlyList<ShopCategoryVm>)ch.Select(Build).ToList()
                : [];
            return new ShopCategoryVm(dto.Id, dto.Slug, dto.Name, dto.Description, dto.ParentCategoryId, children);
        }

        var roots = flat
            .Where(c => !c.ParentCategoryId.HasValue || !knownIds.Contains(c.ParentCategoryId.Value))
            .OrderBy(c => c.Name)
            .Select(Build)
            .ToList();
        return roots;
    }

    // Listing query — returns card-shape data only. Uses GetShopProductCardsQuery
    // which projects everything (price, stock, image, tags) in a single SQL
    // statement family rather than firing GetProductPdpBySlugQuery per product.
    // Partner-aware pricing is overlaid by batching one IPricingService call
    // for all listed SKU ids.
    public async Task<IReadOnlyList<ShopProductCardVm>> GetProductsAsync(
        string? brandSlug = null,
        string? categorySlug = null,
        string? search = null,
        int? partnerId = null,
        CancellationToken ct = default)
    {
        var brandId = await ResolveBrandIdAsync(brandSlug, ct);
        // Resolve the route's category slug to itself + every descendant id
        // so a parent category page shows products from any sub-category.
        // Returns null when no category is specified, [] when the slug doesn't
        // resolve.
        var categoryIds = await ResolveCategoryDescendantIdsAsync(brandSlug, categorySlug, ct);

        // A category was explicitly requested but doesn't match anything in
        // the brand's tree — short-circuit to no products rather than letting
        // the empty-array filter degenerate into "show all".
        if (categoryIds is { Length: 0 }) return [];

        var dtos = await mediator.Send(new GetShopProductCardsQuery(brandId, categoryIds, search), ct);
        if (dtos.Count == 0) return [];

        var cards = dtos
            .Select(d => new ShopProductCardVm(
                d.Id, d.Slug, d.Name, d.Short,
                d.BrandId, d.BrandSlug, d.BrandName,
                d.CategorySlug, d.CategoryName,
                d.Tags, d.MinPrice, d.StockBucket, d.PrimaryImagePath,
                d.SkuIds))
            .ToList();

        if (partnerId.HasValue)
        {
            await OverlayPartnerMinPricesAsync(cards, partnerId.Value, ct);
        }

        return cards;
    }

    // Single batched call to the pricing service for every SKU id across the
    // listing, then recompute MinPrice per card. Each card gets a new copy
    // (records are init-only) with its partner-resolved minimum.
    private async Task OverlayPartnerMinPricesAsync(List<ShopProductCardVm> cards, int partnerId, CancellationToken ct)
    {
        var allSkuIds = cards.SelectMany(c => c.SkuIds).Distinct().ToList();
        if (allSkuIds.Count == 0) return;

        var resolved = await pricing.ResolvePricesAsync(partnerId, allSkuIds, ct);

        for (var i = 0; i < cards.Count; i++)
        {
            var c = cards[i];
            decimal? min = null;
            foreach (var id in c.SkuIds)
            {
                if (resolved.TryGetValue(id, out var price) && price.HasValue)
                {
                    if (!min.HasValue || price.Value < min.Value) min = price.Value;
                }
            }
            if (min.HasValue && min.Value != c.MinPrice)
            {
                cards[i] = c with { MinPrice = min.Value };
            }
        }
    }

    // PDP lookup. Same partner-aware pricing rules as the listing.
    public async Task<ShopProductDetailVm?> GetProductPdpAsync(
        string slug,
        int? partnerId = null,
        CancellationToken ct = default)
    {
        var pdp = await mediator.Send(new GetProductPdpBySlugQuery(slug), ct);
        if (pdp is null) return null;

        var product = ToVm(pdp);

        if (partnerId.HasValue)
        {
            var single = new List<ShopProductVm> { product };
            await OverlayPartnerPricesAsync(single, partnerId.Value, ct);
            product = single[0];
        }

        return new ShopProductDetailVm(
            product,
            pdp.Info.Select(i => new ShopProductInfoVm(i.Key, i.Data)).ToList(),
            pdp.Documents.Select(d => new ShopProductDocVm(d.Name, d.Description, d.Path)).ToList(),
            pdp.Images.Select(i => new ShopProductImageVm(i.Path, i.Description, i.SortOrder)).ToList());
    }

    private static ShopProductVm ToVm(ShopPdpDto pdp)
    {
        var options = pdp.Options
            .Select(o => new ShopOptionVm(o.Id, o.Name, o.Values))
            .ToList();

        var skus = pdp.Skus
            .Select(s => new ShopSkuVm(
                s.Id, s.SKU, s.Opts, s.Price,
                s.StockQuantity, s.LowStockThreshold,
                s.Uom, s.ImagePath))
            .ToList();

        var primaryImage = pdp.Skus.FirstOrDefault(s => !string.IsNullOrEmpty(s.ImagePath))?.ImagePath
                           ?? pdp.Images.FirstOrDefault()?.Path;

        return new ShopProductVm(
            pdp.Id, pdp.Slug,
            pdp.BrandId, pdp.BrandSlug, pdp.BrandName,
            pdp.CategorySlug, pdp.CategoryName,
            pdp.Name, pdp.Short, pdp.Desc,
            pdp.Tags,
            options, skus,
            primaryImage);
    }

    // Overlay partner-resolved prices on each SKU. Build returns a new
    // ShopProductVm because the records are init-only.
    private async Task OverlayPartnerPricesAsync(List<ShopProductVm> products, int partnerId, CancellationToken ct)
    {
        var skuIds = products.SelectMany(p => p.Skus.Select(s => s.Id)).Distinct().ToList();
        if (skuIds.Count == 0) return;

        var resolved = await pricing.ResolvePricesAsync(partnerId, skuIds, ct);

        for (var i = 0; i < products.Count; i++)
        {
            var p = products[i];
            var newSkus = p.Skus
                .Select(s => resolved.TryGetValue(s.Id, out var price) && price.HasValue
                    ? s with { Price = price.Value }
                    : s)
                .ToList();
            products[i] = p with { Skus = newSkus };
        }
    }

    private async Task<int?> ResolveBrandIdAsync(string? brandSlug, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(brandSlug)) return null;
        var brands = await mediator.Send(new GetBrandsQuery(), ct);
        return brands.FirstOrDefault(b => string.Equals(b.Slug, brandSlug, StringComparison.OrdinalIgnoreCase))?.Id;
    }

    // Walks the brand's category tree, finds the requested slug, and returns
    // the category's own id plus every descendant id. Used so /shop/{brand}/{cat}
    // includes products from any sub-category. Returns null when no slug was
    // passed; an empty array when the slug doesn't match any category.
    private async Task<int[]?> ResolveCategoryDescendantIdsAsync(string? brandSlug, string? categorySlug, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(categorySlug)) return null;

        var tree = await GetCategoriesByBrandAsync(ct);
        // Scoped to the brand if known, otherwise search across all brands so
        // the call still works when only categorySlug was supplied.
        IEnumerable<ShopCategoryVm> roots =
            brandSlug is not null && tree.TryGetValue(brandSlug, out var brandTree)
                ? brandTree
                : tree.Values.SelectMany(b => b);

        var found = roots.FindBySlug(categorySlug);
        if (found is null) return [];

        var ids = new List<int>();
        Collect(found, ids);
        return ids.ToArray();

        static void Collect(ShopCategoryVm c, List<int> acc)
        {
            acc.Add(c.Id);
            foreach (var child in c.Children) Collect(child, acc);
        }
    }
}

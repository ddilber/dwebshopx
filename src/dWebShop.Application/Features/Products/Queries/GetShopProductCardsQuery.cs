using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Products.Queries;

// Shop-listing DTO. Computes MinPrice and the in/low/order stock bucket in
// the SQL projection so the catalog/brand/related-strip pages can render
// without firing GetProductPdpBySlugQuery per listed product.
public record ShopProductCardDto(
    int Id,
    string Slug,
    string Name,
    string Short,
    int? BrandId,
    string? BrandSlug,
    string? BrandName,
    string? CategorySlug,
    string? CategoryName,
    List<string> Tags,
    decimal? MinPrice,
    string StockBucket,
    string? PrimaryImagePath,
    List<int> SkuIds);

public record GetShopProductCardsQuery(
    int? BrandId = null,
    // CategoryIds is plural so the shop can pass a parent's id + every
    // descendant id when filtering — picking a parent category in the URL
    // or sidebar then shows products that live in any sub-category.
    int[]? CategoryIds = null,
    string? Search = null) : IRequest<List<ShopProductCardDto>>;

public class GetShopProductCardsQueryHandler(IAppDbContextFactory dbFactory)
    : IRequestHandler<GetShopProductCardsQuery, List<ShopProductCardDto>>
{
    public async Task<List<ShopProductCardDto>> Handle(GetShopProductCardsQuery request, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var query = db.Products
            .AsNoTracking()
            .Where(p => p.Status == ProductStatus.Active);

        if (request.BrandId.HasValue)
            query = query.Where(p => p.BrandId == request.BrandId);

        if (request.CategoryIds is { Length: > 0 })
            query = query.Where(p => p.Categories!.Any(c => request.CategoryIds.Contains(c.Id)));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(s) ||
                p.SKU.ToLower().Contains(s) ||
                p.Description.ToLower().Contains(s));
        }

        // Single projection. The collection-valued projections (Tags, SkuIds)
        // are SplitQuery-friendly — EF emits them as separate small fan-out
        // queries (one per collection), not as N+1 per-row round-trips.
        return await query
            .OrderBy(p => p.Name)
            .Select(p => new ShopProductCardDto(
                p.Id,
                p.Slug,
                p.Name,
                p.Description,
                p.BrandId,
                p.Brand != null ? p.Brand.Slug : null,
                p.Brand != null ? p.Brand.Name : null,
                p.Categories!.OrderBy(c => c.Id).Select(c => c.Slug).FirstOrDefault(),
                p.Categories!.OrderBy(c => c.Id).Select(c => c.Name).FirstOrDefault(),
                p.Tags!.Select(t => t.Name).ToList(),
                // Min base price across active SKUs — null if no SKUs.
                p.ProductSkus!.Min(s => (decimal?)s.Price),
                // Stock bucket aggregation:
                //   any sku with stock > threshold → "in"
                //   else any sku with stock > 0    → "low"
                //   else                            → "order"
                p.ProductSkus!.Any(s => s.StockQuantity > s.LowStockThreshold) ? "in"
                    : p.ProductSkus!.Any(s => s.StockQuantity > 0) ? "low"
                    : "order",
                // Primary image: first SKU image if any, else first product detail image.
                p.ProductSkus!.OrderBy(s => s.Id).Where(s => s.ImagePath != null).Select(s => s.ImagePath).FirstOrDefault()
                    ?? (p.ProductDetails != null
                        ? p.ProductDetails.Images!.OrderBy(i => i.SortOrder).Select(i => i.Path).FirstOrDefault()
                        : null),
                p.ProductSkus!.Select(s => s.Id).ToList()))
            .ToListAsync(ct);
    }
}

using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Products.Queries;

// Shop-specific PDP shape. Returns everything the public product detail page
// needs in one round-trip: header, body, gallery, docs, options, SKUs (with
// per-SKU stock + base price), and the option-value matrix for variant picking.
//
// This is separate from GetProductByIdQuery / GetProductBySlugQuery (which
// power the Admin product editor) so that the Admin DTO can keep evolving
// without affecting the shop and vice-versa.

public record ShopPdpInfoDto(int Id, string Key, string Data);
public record ShopPdpDocDto(int Id, string Name, string Path, string Description);
public record ShopPdpImageDto(int Id, string Path, string Description, int SortOrder);
public record ShopPdpOptionDto(int Id, string Name, List<string> Values);
public record ShopPdpSkuDto(
    int Id,
    string SKU,
    decimal Price,
    int StockQuantity,
    int LowStockThreshold,
    string Uom,
    string? ImagePath,
    // Opts is the SKU's option values in the SAME order as the parent product's
    // Options list. Used by the page to match (Option name -> selected value).
    List<string> Opts);

public record ShopPdpDto(
    int Id,
    string Slug,
    string Name,
    string Short,
    string Desc,
    bool IsFeatured,
    int? BrandId,
    string? BrandSlug,
    string? BrandName,
    string? CategorySlug,
    string? CategoryName,
    List<string> Tags,
    List<ShopPdpImageDto> Images,
    List<ShopPdpDocDto> Documents,
    List<ShopPdpInfoDto> Info,
    List<ShopPdpOptionDto> Options,
    List<ShopPdpSkuDto> Skus);

public record GetProductPdpBySlugQuery(string Slug) : IRequest<ShopPdpDto?>;

public class GetProductPdpBySlugQueryHandler(IAppDbContext db)
    : IRequestHandler<GetProductPdpBySlugQuery, ShopPdpDto?>
{
    public async Task<ShopPdpDto?> Handle(GetProductPdpBySlugQuery request, CancellationToken ct)
    {
        var p = await db.Products
            .AsNoTracking()
            .Include(x => x.Brand)
            .Include(x => x.Categories)
            .Include(x => x.Tags)
            .Include(x => x.ProductDetails)
                .ThenInclude(pd => pd!.Images)
            .Include(x => x.ProductDetails)
                .ThenInclude(pd => pd!.Documents)
            .Include(x => x.ProductDetails)
                .ThenInclude(pd => pd!.Information)
            .Include(x => x.ProductOptions!)
                .ThenInclude(o => o.ProductOptionValues)
            .Include(x => x.ProductSkus!)
                .ThenInclude(s => s.SkuOptionValues!)
                    .ThenInclude(sov => sov.ProductOptionValue)
            .FirstOrDefaultAsync(x => x.Slug == request.Slug && x.Status == ProductStatus.Active, ct);

        if (p is null) return null;

        // Stable option ordering by Id — so the page's option columns stay in
        // a deterministic order across requests.
        var options = (p.ProductOptions ?? [])
            .OrderBy(o => o.Id)
            .ToList();

        var optionDtos = options
            .Select(o => new ShopPdpOptionDto(
                o.Id,
                o.Name,
                (o.ProductOptionValues ?? [])
                    .OrderBy(v => v.Id)
                    .Select(v => v.Name)
                    .ToList()))
            .ToList();

        // Build SKU dtos with Opts[] aligned to the option ordering above.
        var skuDtos = (p.ProductSkus ?? [])
            .OrderBy(s => s.Id)
            .Select(s =>
            {
                // (optionId -> selected value name) for this SKU
                var byOption = (s.SkuOptionValues ?? [])
                    .Where(sov => sov.ProductOptionsId.HasValue && sov.ProductOptionValue is not null)
                    .ToDictionary(
                        sov => sov.ProductOptionsId!.Value,
                        sov => sov.ProductOptionValue!.Name);

                var opts = options
                    .Select(o => byOption.TryGetValue(o.Id, out var v) ? v : string.Empty)
                    .ToList();

                return new ShopPdpSkuDto(
                    s.Id,
                    s.SKU,
                    s.Price,
                    s.StockQuantity,
                    s.LowStockThreshold,
                    s.Uom,
                    s.ImagePath,
                    opts);
            })
            .ToList();

        // Tags: the seed currently uses a string-joined Tags column on Product
        // for many entities, but Product also has a many-to-many Tags nav.
        // Prefer the nav if present; the page only needs display strings.
        var tagNames = (p.Tags ?? []).Select(t => t.Name).ToList();

        // Pick a primary category for breadcrumb purposes — first by id is fine
        // since the static data only ever assigned one category per product.
        var primaryCategory = (p.Categories ?? []).OrderBy(c => c.Id).FirstOrDefault();

        var pd = p.ProductDetails;
        return new ShopPdpDto(
            p.Id,
            p.Slug,
            p.Name,
            p.Description,
            pd?.DetailDescription ?? string.Empty,
            p.IsFeatured,
            p.BrandId,
            p.Brand?.Slug,
            p.Brand?.Name,
            primaryCategory?.Slug,
            primaryCategory?.Name,
            tagNames,
            pd?.Images?.OrderBy(i => i.SortOrder)
                .Select(i => new ShopPdpImageDto(i.Id, i.Path, i.Description, i.SortOrder))
                .ToList() ?? [],
            pd?.Documents?
                .Select(d => new ShopPdpDocDto(d.Id, d.Name, d.Path, d.Description))
                .ToList() ?? [],
            pd?.Information?
                .Select(i => new ShopPdpInfoDto(i.Id, i.Key, i.Data))
                .ToList() ?? [],
            optionDtos,
            skuDtos);
    }
}

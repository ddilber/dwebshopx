using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Products.Queries;

public record GetProductBySlugQuery(string Slug) : IRequest<ProductDetailDto?>;

public class GetProductBySlugQueryHandler(IAppDbContext db) : IRequestHandler<GetProductBySlugQuery, ProductDetailDto?>
{
    public async Task<ProductDetailDto?> Handle(GetProductBySlugQuery request, CancellationToken ct)
    {
        var p = await db.Products
            .Include(x => x.Brand)
            .Include(x => x.ProductDetails)
                .ThenInclude(pd => pd!.Images)
            .Include(x => x.ProductDetails)
                .ThenInclude(pd => pd!.Documents)
            .Include(x => x.ProductDetails)
                .ThenInclude(pd => pd!.Information)
            .Include(x => x.Categories)
            .FirstOrDefaultAsync(x => x.Slug == request.Slug, ct);

        if (p is null) return null;

        var pd = p.ProductDetails;
        return new ProductDetailDto(
            p.Id, p.Name, p.SKU, p.ExtRef, p.Slug, p.Description, p.IsActive,
            p.BrandId, p.Brand?.Name,
            pd?.DetailDescription ?? string.Empty,
            pd?.Images?.Select(i => new ProductImageDto(i.Id, i.Path, i.Description)).ToList() ?? [],
            pd?.Documents?.Select(d => new ProductDocumentDto(d.Id, d.Name, d.Path, d.Description)).ToList() ?? [],
            pd?.Information?.Select(i => new ProductInfoDto(i.Id, i.Key, i.Data)).ToList() ?? [],
            p.Categories?.Select(c => new CategoryRefDto(c.Id, c.Name)).ToList() ?? []);
    }
}

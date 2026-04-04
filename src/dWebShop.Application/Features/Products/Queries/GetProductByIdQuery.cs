using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Products.Queries;

public record ProductImageDto(int Id, string Path, string Description);
public record ProductDocumentDto(int Id, string Name, string Path, string Description);
public record ProductInfoDto(int Id, string Key, string Data);
public record CategoryRefDto(int Id, string Name);

public record ProductDetailDto(
    int Id, string Name, string SKU, string ExtRef, string Slug, string Description, bool IsActive,
    int? BrandId, string? BrandName,
    string DetailDescription,
    List<ProductImageDto> Images,
    List<ProductDocumentDto> Documents,
    List<ProductInfoDto> Infos,
    List<CategoryRefDto> Categories);

public record GetProductByIdQuery(int Id) : IRequest<ProductDetailDto?>;

public class GetProductByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetProductByIdQuery, ProductDetailDto?>
{
    public async Task<ProductDetailDto?> Handle(GetProductByIdQuery request, CancellationToken ct)
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
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

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

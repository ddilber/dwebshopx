using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Brands.Queries;

public record BrandDto(int Id, string Name, string Slug, string Description, string LogoImage, string SliderImage);

public record GetBrandsQuery : IRequest<List<BrandDto>>;

public class GetBrandsQueryHandler(IAppDbContext db) : IRequestHandler<GetBrandsQuery, List<BrandDto>>
{
    public async Task<List<BrandDto>> Handle(GetBrandsQuery request, CancellationToken ct) =>
        await db.Brands
            .OrderBy(b => b.Name)
            .Select(b => new BrandDto(b.Id, b.Name, b.Slug, b.Description, b.LogoImage, b.SliderImage))
            .ToListAsync(ct);
}

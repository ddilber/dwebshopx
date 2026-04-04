using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Brands.Queries;

public record BrandWithCategoriesDto(
    int Id, string Name, string Slug, string Description, string LogoImage, string SliderImage,
    List<CategorySummaryDto> Categories);

public record CategorySummaryDto(int Id, string Name, string Slug, string Description, int? ParentCategoryId);

public record GetBrandBySlugQuery(string Slug) : IRequest<BrandWithCategoriesDto?>;

public class GetBrandBySlugQueryHandler(IAppDbContext db) : IRequestHandler<GetBrandBySlugQuery, BrandWithCategoriesDto?>
{
    public async Task<BrandWithCategoriesDto?> Handle(GetBrandBySlugQuery request, CancellationToken ct)
    {
        var brand = await db.Brands
            .AsNoTracking()
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.Slug == request.Slug, ct);

        if (brand is null) return null;

        return new BrandWithCategoriesDto(
            brand.Id, brand.Name, brand.Slug, brand.Description, brand.LogoImage, brand.SliderImage,
            brand.Categories?
                .OrderBy(c => c.CategoryId)
                .ThenBy(c => c.Name)
                .Select(c => new CategorySummaryDto(c.Id, c.Name, c.Slug, c.Description, c.CategoryId))
                .ToList() ?? []);
    }
}

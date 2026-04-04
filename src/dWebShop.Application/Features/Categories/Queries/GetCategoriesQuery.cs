using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Categories.Queries;

public record CategoryDto(int Id, string Name, string Slug, string Description, int? ParentCategoryId, string? ParentName, int? BrandId, string? BrandName);

public record GetCategoriesQuery(int? BrandId = null) : IRequest<List<CategoryDto>>;

public class GetCategoriesQueryHandler(IAppDbContext db) : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        var query = db.Categories
            .Include(c => c.Brand)
            .Include(c => c.ParentCategory)
            .AsQueryable();

        if (request.BrandId.HasValue)
            query = query.Where(c => c.BrandId == request.BrandId);

        return await query
            .OrderBy(c => c.BrandId)
            .ThenBy(c => c.CategoryId)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto(
                c.Id, c.Name, c.Slug, c.Description,
                c.CategoryId, c.ParentCategory != null ? c.ParentCategory.Name : null,
                c.BrandId, c.Brand != null ? c.Brand.Name : null))
            .ToListAsync(ct);
    }
}

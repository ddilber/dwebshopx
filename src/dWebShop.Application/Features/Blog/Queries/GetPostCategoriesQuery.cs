using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Blog.Queries;

public record PostCategoryDto(int Id, string Name, string Slug);

public record GetPostCategoriesQuery : IRequest<List<PostCategoryDto>>;

public class GetPostCategoriesQueryHandler(IAppDbContext db) : IRequestHandler<GetPostCategoriesQuery, List<PostCategoryDto>>
{
    public async Task<List<PostCategoryDto>> Handle(GetPostCategoriesQuery request, CancellationToken ct) =>
        await db.PostCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new PostCategoryDto(c.Id, c.Name, c.Slug))
            .ToListAsync(ct);
}

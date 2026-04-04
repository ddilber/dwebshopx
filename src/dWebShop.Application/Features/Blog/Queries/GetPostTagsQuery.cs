using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Blog.Queries;

public record PostTagDto(int Id, string Name, string Slug);

public record GetPostTagsQuery : IRequest<List<PostTagDto>>;

public class GetPostTagsQueryHandler(IAppDbContext db) : IRequestHandler<GetPostTagsQuery, List<PostTagDto>>
{
    public async Task<List<PostTagDto>> Handle(GetPostTagsQuery request, CancellationToken ct) =>
        await db.PostTags
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new PostTagDto(t.Id, t.Name, t.Slug))
            .ToListAsync(ct);
}

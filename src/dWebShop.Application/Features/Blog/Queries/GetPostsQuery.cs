using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Blog.Queries;

public record PostListItemDto(
    int Id,
    string Title,
    string Slug,
    string Summary,
    bool Published,
    DateTime? CreatedDate,
    List<string> CategoryNames,
    List<string> TagNames);

public record PostDto(
    int Id,
    string Title,
    string MetaTitle,
    string Slug,
    string Summary,
    string Content,
    bool Published,
    DateTime? CreatedDate,
    List<PostCategoryDto> Categories,
    List<PostTagDto> Tags,
    string MetaDescription);

// Admin: all posts, with optional status filter
public record GetPostsQuery(bool? Published = null, string? Search = null) : IRequest<List<PostListItemDto>>;

public class GetPostsQueryHandler(IAppDbContext db) : IRequestHandler<GetPostsQuery, List<PostListItemDto>>
{
    public async Task<List<PostListItemDto>> Handle(GetPostsQuery request, CancellationToken ct)
    {
        var query = db.Posts
            .AsNoTracking()
            .Include(p => p.Categories)
            .Include(p => p.Tags)
            .AsQueryable();

        if (request.Published.HasValue)
            query = query.Where(p => p.Published == request.Published.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => p.Title.Contains(request.Search) || p.Slug.Contains(request.Search));

        return await query
            .OrderByDescending(p => p.CreatedDate)
            .Select(p => new PostListItemDto(
                p.Id,
                p.Title,
                p.Slug,
                p.Summary,
                p.Published,
                p.CreatedDate,
                p.Categories!.Select(c => c.Name).ToList(),
                p.Tags!.Select(t => t.Name).ToList()))
            .ToListAsync(ct);
    }
}

// Public: published posts, paginated, with optional category/tag filter
public record GetPublishedPostsQuery(int Page = 1, int PageSize = 10, string? CategorySlug = null, string? TagSlug = null)
    : IRequest<PagedPostsResult>;

public record PagedPostsResult(List<PostListItemDto> Items, int TotalCount, int Page, int PageSize);

public class GetPublishedPostsQueryHandler(IAppDbContext db) : IRequestHandler<GetPublishedPostsQuery, PagedPostsResult>
{
    public async Task<PagedPostsResult> Handle(GetPublishedPostsQuery request, CancellationToken ct)
    {
        var query = db.Posts
            .AsNoTracking()
            .Where(p => p.Published)
            .Include(p => p.Categories)
            .Include(p => p.Tags)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.CategorySlug))
            query = query.Where(p => p.Categories!.Any(c => c.Slug == request.CategorySlug));

        if (!string.IsNullOrWhiteSpace(request.TagSlug))
            query = query.Where(p => p.Tags!.Any(t => t.Slug == request.TagSlug));

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PostListItemDto(
                p.Id,
                p.Title,
                p.Slug,
                p.Summary,
                p.Published,
                p.CreatedDate,
                p.Categories!.Select(c => c.Name).ToList(),
                p.Tags!.Select(t => t.Name).ToList()))
            .ToListAsync(ct);

        return new PagedPostsResult(items, total, request.Page, request.PageSize);
    }
}

// Single post by slug (public)
public record GetPostBySlugQuery(string Slug) : IRequest<PostDto?>;

public class GetPostBySlugQueryHandler(IAppDbContext db) : IRequestHandler<GetPostBySlugQuery, PostDto?>
{
    public async Task<PostDto?> Handle(GetPostBySlugQuery request, CancellationToken ct)
    {
        var p = await db.Posts
            .AsNoTracking()
            .Where(x => x.Slug == request.Slug && x.Published)
            .Include(x => x.Categories)
            .Include(x => x.Tags)
            .Include(x => x.Metas)
            .FirstOrDefaultAsync(ct);

        if (p is null) return null;

        var metaDesc = p.Metas?.FirstOrDefault(m => m.Key == "description")?.Content ?? string.Empty;

        return new PostDto(
            p.Id,
            p.Title,
            p.MetaTitle,
            p.Slug,
            p.Summary,
            p.Content,
            p.Published,
            p.CreatedDate,
            p.Categories!.Select(c => new PostCategoryDto(c.Id, c.Name, c.Slug)).ToList(),
            p.Tags!.Select(t => new PostTagDto(t.Id, t.Name, t.Slug)).ToList(),
            metaDesc);
    }
}

// Single post by id (admin)
public record GetPostByIdQuery(int Id) : IRequest<PostDto?>;

public class GetPostByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetPostByIdQuery, PostDto?>
{
    public async Task<PostDto?> Handle(GetPostByIdQuery request, CancellationToken ct)
    {
        var p = await db.Posts
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Include(x => x.Categories)
            .Include(x => x.Tags)
            .Include(x => x.Metas)
            .FirstOrDefaultAsync(ct);

        if (p is null) return null;

        var metaDesc = p.Metas?.FirstOrDefault(m => m.Key == "description")?.Content ?? string.Empty;

        return new PostDto(
            p.Id,
            p.Title,
            p.MetaTitle,
            p.Slug,
            p.Summary,
            p.Content,
            p.Published,
            p.CreatedDate,
            p.Categories!.Select(c => new PostCategoryDto(c.Id, c.Name, c.Slug)).ToList(),
            p.Tags!.Select(t => new PostTagDto(t.Id, t.Name, t.Slug)).ToList(),
            metaDesc);
    }
}

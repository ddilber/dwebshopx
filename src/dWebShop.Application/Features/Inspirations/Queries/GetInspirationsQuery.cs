using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Inspirations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace dWebShop.Application.Features.Inspirations.Queries;

public record InspirationSectionDto(
    string Type,
    string? Text = null,
    string? Label = null,
    string[]? Items = null);

public record InspirationListItemDto(
    int Id,
    string Slug,
    string BrandSlug,
    InspirationContentType ContentType,
    bool IsFeatured,
    bool Published,
    string Title,
    string Lede,
    string HeroLabel,
    string PublishedAt,
    int ReadMin,
    string[] Authors,
    string[] Tags);

public record InspirationDetailDto(
    int Id,
    string Slug,
    string BrandSlug,
    InspirationContentType ContentType,
    bool IsFeatured,
    bool Published,
    string Title,
    string Lede,
    string HeroLabel,
    string PublishedAt,
    int ReadMin,
    string[] Authors,
    string[] Tags,
    InspirationSectionDto[] Sections,
    string[] LinkedProductSlugs);

// Admin: all inspirations with optional brand/published filter
public record GetInspirationsQuery(string? BrandSlug = null, bool? Published = null, string? Search = null)
    : IRequest<List<InspirationListItemDto>>;

public class GetInspirationsQueryHandler(IAppDbContext db) : IRequestHandler<GetInspirationsQuery, List<InspirationListItemDto>>
{
    public async Task<List<InspirationListItemDto>> Handle(GetInspirationsQuery request, CancellationToken ct)
    {
        var query = db.Inspirations
            .AsNoTracking()
            .Include(x => x.Brand)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.BrandSlug))
            query = query.Where(x => x.Brand != null && x.Brand.Slug == request.BrandSlug);

        if (request.Published.HasValue)
            query = query.Where(x => x.Published == request.Published.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(x => x.Title.Contains(request.Search) || x.Slug.Contains(request.Search));

        var rows = await query
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new
            {
                x.Id, x.Slug, BrandSlug = x.Brand != null ? x.Brand.Slug : string.Empty,
                x.ContentType, x.IsFeatured, x.Published, x.Title, x.Lede,
                x.HeroLabel, x.PublishedAt, x.ReadMin, x.Authors, x.Tags
            })
            .ToListAsync(ct);

        return rows.Select(x => new InspirationListItemDto(
            x.Id, x.Slug, x.BrandSlug, x.ContentType, x.IsFeatured, x.Published,
            x.Title, x.Lede, x.HeroLabel, x.PublishedAt, x.ReadMin,
            string.IsNullOrEmpty(x.Authors) ? [] : x.Authors.Split('|'),
            string.IsNullOrEmpty(x.Tags) ? [] : x.Tags.Split('|'))).ToList();
    }
}

// Public: published inspirations for a brand, ordered by featured then date
public record GetPublishedInspirationsByBrandQuery(string BrandSlug) : IRequest<List<InspirationListItemDto>>;

public class GetPublishedInspirationsByBrandQueryHandler(IAppDbContext db)
    : IRequestHandler<GetPublishedInspirationsByBrandQuery, List<InspirationListItemDto>>
{
    public async Task<List<InspirationListItemDto>> Handle(GetPublishedInspirationsByBrandQuery request, CancellationToken ct)
    {
        var rows = await db.Inspirations
            .AsNoTracking()
            .Include(x => x.Brand)
            .Where(x => x.Published && x.Brand != null && x.Brand.Slug == request.BrandSlug)
            .OrderByDescending(x => x.IsFeatured)
            .ThenByDescending(x => x.CreatedDate)
            .Select(x => new
            {
                x.Id, x.Slug, BrandSlug = x.Brand != null ? x.Brand.Slug : string.Empty,
                x.ContentType, x.IsFeatured, x.Published, x.Title, x.Lede,
                x.HeroLabel, x.PublishedAt, x.ReadMin, x.Authors, x.Tags
            })
            .ToListAsync(ct);

        return rows.Select(x => new InspirationListItemDto(
            x.Id, x.Slug, x.BrandSlug, x.ContentType, x.IsFeatured, x.Published,
            x.Title, x.Lede, x.HeroLabel, x.PublishedAt, x.ReadMin,
            string.IsNullOrEmpty(x.Authors) ? [] : x.Authors.Split('|'),
            string.IsNullOrEmpty(x.Tags) ? [] : x.Tags.Split('|'))).ToList();
    }
}

// Single article by brand slug + article slug (public & admin)
public record GetInspirationBySlugQuery(string BrandSlug, string Slug) : IRequest<InspirationDetailDto?>;

public class GetInspirationBySlugQueryHandler(IAppDbContext db) : IRequestHandler<GetInspirationBySlugQuery, InspirationDetailDto?>
{
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public async Task<InspirationDetailDto?> Handle(GetInspirationBySlugQuery request, CancellationToken ct)
    {
        var x = await db.Inspirations
            .AsNoTracking()
            .Include(i => i.Brand)
            .FirstOrDefaultAsync(i =>
                i.Slug == request.Slug &&
                i.Brand != null && i.Brand.Slug == request.BrandSlug, ct);

        if (x is null) return null;

        var sections = string.IsNullOrWhiteSpace(x.Content)
            ? Array.Empty<InspirationSectionDto>()
            : JsonSerializer.Deserialize<InspirationSectionDto[]>(x.Content, _json)
              ?? Array.Empty<InspirationSectionDto>();

        return new InspirationDetailDto(
            x.Id,
            x.Slug,
            x.Brand?.Slug ?? string.Empty,
            x.ContentType,
            x.IsFeatured,
            x.Published,
            x.Title,
            x.Lede,
            x.HeroLabel,
            x.PublishedAt,
            x.ReadMin,
            x.Authors == string.Empty ? Array.Empty<string>() : x.Authors.Split('|'),
            x.Tags == string.Empty ? Array.Empty<string>() : x.Tags.Split('|'),
            sections,
            x.LinkedProductSlugs == string.Empty ? Array.Empty<string>() : x.LinkedProductSlugs.Split('|'));
    }
}

// Single article by id (admin edit)
public record GetInspirationByIdQuery(int Id) : IRequest<InspirationDetailDto?>;

public class GetInspirationByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetInspirationByIdQuery, InspirationDetailDto?>
{
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public async Task<InspirationDetailDto?> Handle(GetInspirationByIdQuery request, CancellationToken ct)
    {
        var x = await db.Inspirations
            .AsNoTracking()
            .Include(i => i.Brand)
            .FirstOrDefaultAsync(i => i.Id == request.Id, ct);

        if (x is null) return null;

        var sections = string.IsNullOrWhiteSpace(x.Content)
            ? Array.Empty<InspirationSectionDto>()
            : JsonSerializer.Deserialize<InspirationSectionDto[]>(x.Content, _json)
              ?? Array.Empty<InspirationSectionDto>();

        return new InspirationDetailDto(
            x.Id,
            x.Slug,
            x.Brand?.Slug ?? string.Empty,
            x.ContentType,
            x.IsFeatured,
            x.Published,
            x.Title,
            x.Lede,
            x.HeroLabel,
            x.PublishedAt,
            x.ReadMin,
            x.Authors == string.Empty ? Array.Empty<string>() : x.Authors.Split('|'),
            x.Tags == string.Empty ? Array.Empty<string>() : x.Tags.Split('|'),
            sections,
            x.LinkedProductSlugs == string.Empty ? Array.Empty<string>() : x.LinkedProductSlugs.Split('|'));
    }
}


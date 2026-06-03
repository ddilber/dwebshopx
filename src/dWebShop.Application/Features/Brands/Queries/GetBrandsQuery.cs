using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace dWebShop.Application.Features.Brands.Queries;

public record BrandDto(int Id, string Name, string Slug, string Description, string LogoImage, string SliderImage);

public record GetBrandsQuery : IRequest<List<BrandDto>>;

// Uses IAppDbContextFactory rather than the scoped IAppDbContext because
// ShopCatalogService fans out three parallel queries (brands, categories,
// products) and each of them transitively calls GetBrandsQuery. With a
// scoped context those parallel calls clashed on the same connection.
// Cache hit returns instantly; cache miss runs on a fresh context.
public class GetBrandsQueryHandler(IAppDbContextFactory dbFactory, IMemoryCache cache) : IRequestHandler<GetBrandsQuery, List<BrandDto>>
{
    private const string CacheKey = "brands:all";

    public async Task<List<BrandDto>> Handle(GetBrandsQuery request, CancellationToken ct)
    {
        if (cache.TryGetValue(CacheKey, out List<BrandDto>? cached) && cached is not null)
            return cached;

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var brands = await db.Brands
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(b => new BrandDto(b.Id, b.Name, b.Slug, b.Description, b.LogoImage, b.SliderImage))
            .ToListAsync(ct);

        cache.Set(CacheKey, brands, TimeSpan.FromMinutes(5));
        return brands;
    }
}

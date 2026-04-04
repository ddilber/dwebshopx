using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Products.Queries;

public record ProductListItemDto(int Id, string Name, string SKU, string Slug, bool IsActive, int? BrandId, string? BrandName, string? PrimaryImage, decimal? MinPrice = null);

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);

public record GetProductsQuery(
    int Page = 1,
    int PageSize = 20,
    int? BrandId = null,
    int? CategoryId = null,
    string? Search = null,
    int[]? BrandIds = null,
    int[]? CategoryIds = null,
    bool IncludePrice = false) : IRequest<PagedResult<ProductListItemDto>>;

public class GetProductsQueryHandler(IAppDbContext db) : IRequestHandler<GetProductsQuery, PagedResult<ProductListItemDto>>
{
    public async Task<PagedResult<ProductListItemDto>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var query = db.Products
            .Include(p => p.Brand)
            .Include(p => p.ProductDetails)
                .ThenInclude(pd => pd!.Images)
            .AsQueryable();

        if (request.BrandIds?.Length > 0)
            query = query.Where(p => p.BrandId.HasValue && request.BrandIds.Contains(p.BrandId.Value));
        else if (request.BrandId.HasValue)
            query = query.Where(p => p.BrandId == request.BrandId);

        if (request.CategoryIds?.Length > 0)
            query = query.Where(p => p.Categories!.Any(c => request.CategoryIds.Contains(c.Id)));
        else if (request.CategoryId.HasValue)
            query = query.Where(p => p.Categories!.Any(c => c.Id == request.CategoryId));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s) || p.SKU.ToLower().Contains(s) || p.Description.ToLower().Contains(s));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductListItemDto(
                p.Id, p.Name, p.SKU, p.Slug, p.IsActive,
                p.BrandId, p.Brand != null ? p.Brand.Name : null,
                p.ProductDetails != null && p.ProductDetails.Images!.Any()
                    ? p.ProductDetails.Images!.First().Path
                    : null,
                request.IncludePrice
                    ? db.ProductSkus.Where(s => s.ProductId == p.Id).Min(s => (decimal?)s.Price)
                    : null))
            .ToListAsync(ct);

        return new PagedResult<ProductListItemDto>(items, total, request.Page, request.PageSize);
    }
}

using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.ProductSkus.Queries;

public record SkuOptionValueDto(int Id, string OptionName, string OptionValueName);
public record ProductSkuDto(int Id, string SKU, string ExtRef, string Name, string Gtin, decimal Price, decimal? CompareAtPrice, decimal? CostPrice, decimal Tax, int StockQuantity, int LowStockThreshold, string? ImagePath, List<SkuOptionValueDto> Options);
public record ProductOptionValueDto(int Id, string Name);
public record ProductOptionDto(int Id, string Name, bool IsNamePart, List<ProductOptionValueDto> Values);

public record GetProductSkusQuery(int ProductId) : IRequest<GetProductSkusResult>;

public record GetProductSkusResult(List<ProductSkuDto> Skus, List<ProductOptionDto> Options);

public class GetProductSkusQueryHandler(IAppDbContext db) : IRequestHandler<GetProductSkusQuery, GetProductSkusResult>
{
    public async Task<GetProductSkusResult> Handle(GetProductSkusQuery request, CancellationToken ct)
    {
        var skus = await db.ProductSkus
            .AsNoTracking()
            .Include(s => s.SkuOptionValues!)
                .ThenInclude(sov => sov.ProductOption)
            .Include(s => s.SkuOptionValues!)
                .ThenInclude(sov => sov.ProductOptionValue)
            .Where(s => s.ProductId == request.ProductId)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

        var options = await db.ProductOptions
            .AsNoTracking()
            .Include(o => o.ProductOptionValues)
            .Where(o => o.ProductId == request.ProductId)
            .OrderBy(o => o.Name)
            .ToListAsync(ct);

        var skuDtos = skus.Select(s => new ProductSkuDto(
            s.Id, s.SKU, s.ExtRef, s.Name, s.Gtin, s.Price, s.CompareAtPrice, s.CostPrice, s.Tax, s.StockQuantity, s.LowStockThreshold, s.ImagePath,
            s.SkuOptionValues?.Select(sov => new SkuOptionValueDto(
                sov.Id,
                sov.ProductOption?.Name ?? string.Empty,
                sov.ProductOptionValue?.Name ?? string.Empty)).ToList() ?? []
        )).ToList();

        var optionDtos = options.Select(o => new ProductOptionDto(
            o.Id, o.Name, o.IsNamePart,
            o.ProductOptionValues?.Select(v => new ProductOptionValueDto(v.Id, v.Name)).ToList() ?? []
        )).ToList();

        return new GetProductSkusResult(skuDtos, optionDtos);
    }
}

public record GetAllSkusQuery : IRequest<List<ProductSkuDto>>;

public class GetAllSkusQueryHandler(IAppDbContext db) : IRequestHandler<GetAllSkusQuery, List<ProductSkuDto>>
{
    public async Task<List<ProductSkuDto>> Handle(GetAllSkusQuery request, CancellationToken ct) =>
        await db.ProductSkus
            .AsNoTracking()
            .OrderBy(s => s.SKU)
            .Select(s => new ProductSkuDto(s.Id, s.SKU, s.ExtRef, s.Name, s.Gtin, s.Price, s.CompareAtPrice, s.CostPrice, s.Tax, s.StockQuantity, s.LowStockThreshold, s.ImagePath, new List<SkuOptionValueDto>()))
            .ToListAsync(ct);
}

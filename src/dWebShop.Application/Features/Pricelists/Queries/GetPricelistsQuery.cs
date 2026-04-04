using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Pricelists.Queries;

public record PricelistItemDto(int Id, int PricelistId, int? ProductSkuId, string SkuName, string SkuCode, decimal Price, decimal? MinQuantity);
public record ClientPricelistDto(int Id, int PartnerId, string PartnerName, int PricelistId, bool IsDefault);
public record PricelistDto(int Id, string Name, string Description, bool IsActive, List<PricelistItemDto> Items, List<ClientPricelistDto> Clients);
public record PricelistSummaryDto(int Id, string Name, string Description, bool IsActive, int ItemCount, int ClientCount);

public record GetPricelistsQuery : IRequest<List<PricelistSummaryDto>>;

public class GetPricelistsQueryHandler(IAppDbContext db) : IRequestHandler<GetPricelistsQuery, List<PricelistSummaryDto>>
{
    public async Task<List<PricelistSummaryDto>> Handle(GetPricelistsQuery request, CancellationToken ct) =>
        await db.Pricelists
            .OrderBy(p => p.Name)
            .Select(p => new PricelistSummaryDto(
                p.Id, p.Name, p.Description, p.IsActive,
                p.Items != null ? p.Items.Count : 0,
                p.ClientPricelists != null ? p.ClientPricelists.Count : 0))
            .ToListAsync(ct);
}

public record GetPricelistByIdQuery(int Id) : IRequest<PricelistDto?>;

public class GetPricelistByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetPricelistByIdQuery, PricelistDto?>
{
    public async Task<PricelistDto?> Handle(GetPricelistByIdQuery request, CancellationToken ct)
    {
        var pl = await db.Pricelists
            .Include(p => p.Items!)
                .ThenInclude(i => i.ProductSku)
            .Include(p => p.ClientPricelists!)
                .ThenInclude(cp => cp.Partner)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

        if (pl is null) return null;

        return new PricelistDto(
            pl.Id, pl.Name, pl.Description, pl.IsActive,
            pl.Items?.Select(i => new PricelistItemDto(
                i.Id, i.PricelistId, i.ProductSkuId,
                i.ProductSku?.Name ?? string.Empty,
                i.ProductSku?.SKU ?? string.Empty,
                i.Price, i.MinQuantity)).ToList() ?? [],
            pl.ClientPricelists?.Select(cp => new ClientPricelistDto(
                cp.Id, cp.PartnerId,
                cp.Partner != null ? $"{cp.Partner.FirstName} {cp.Partner.LastName} ({cp.Partner.CompanyName})".Trim() : string.Empty,
                cp.PricelistId, cp.IsDefault)).ToList() ?? []);
    }
}

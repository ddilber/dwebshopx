using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Partners;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Partners.Queries;

public record PartnerDto(int Id, string DisplayName, string Email);
public record ClientDiscountInfoDto(int Id, int PartnerId, decimal DiscountPercent, string? Description, DateTime? ValidFrom, DateTime? ValidTo);

public record GetPartnersQuery : IRequest<List<PartnerDto>>;

public class GetPartnersQueryHandler(IAppDbContext db) : IRequestHandler<GetPartnersQuery, List<PartnerDto>>
{
    public async Task<List<PartnerDto>> Handle(GetPartnersQuery request, CancellationToken ct) =>
        await db.Partners
            .OrderBy(p => p.CompanyName).ThenBy(p => p.LastName)
            .Select(p => new PartnerDto(
                p.Id,
                (p.CompanyName != "" ? p.CompanyName + " — " : "") + p.FirstName + " " + p.LastName,
                p.Email))
            .ToListAsync(ct);
}

public record GetClientDiscountsQuery : IRequest<List<ClientDiscountInfoDto>>;

public class GetClientDiscountsQueryHandler(IAppDbContext db) : IRequestHandler<GetClientDiscountsQuery, List<ClientDiscountInfoDto>>
{
    public async Task<List<ClientDiscountInfoDto>> Handle(GetClientDiscountsQuery request, CancellationToken ct) =>
        await db.ClientDiscounts
            .OrderBy(d => d.PartnerId)
            .Select(d => new ClientDiscountInfoDto(d.Id, d.PartnerId, d.DiscountPercent, d.Description, d.ValidFrom, d.ValidTo))
            .ToListAsync(ct);
}

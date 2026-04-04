using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Profile.Queries;

public record GetProfileQuery(int PartnerId) : IRequest<ProfileDto?>;

public record ProfileDto(
    int PartnerId,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    List<DeliveryAddressDto> DeliveryAddresses);

public record DeliveryAddressDto(
    int Id,
    string Label,
    string Address1,
    string ZipCode,
    string City,
    string Country);

public class GetProfileQueryHandler(IAppDbContext db) : IRequestHandler<GetProfileQuery, ProfileDto?>
{
    public async Task<ProfileDto?> Handle(GetProfileQuery request, CancellationToken ct)
    {
        var partner = await db.Partners
            .Include(p => p.DeliveryAddresses)
            .FirstOrDefaultAsync(p => p.Id == request.PartnerId, ct);

        if (partner is null) return null;

        var addresses = partner.DeliveryAddresses?
            .Select(a => new DeliveryAddressDto(a.Id, a.Label, a.Address1, a.ZipCode, a.City, a.Country))
            .ToList() ?? [];

        return new ProfileDto(partner.Id, partner.FirstName, partner.LastName, partner.Email, partner.Phone, addresses);
    }
}

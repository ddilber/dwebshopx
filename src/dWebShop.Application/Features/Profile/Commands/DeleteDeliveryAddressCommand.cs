using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Profile.Commands;

public record DeleteDeliveryAddressCommand(int AddressId, int PartnerId) : IRequest;

public class DeleteDeliveryAddressCommandHandler(IAppDbContext db) : IRequestHandler<DeleteDeliveryAddressCommand>
{
    public async Task Handle(DeleteDeliveryAddressCommand request, CancellationToken ct)
    {
        var partner = await db.Partners
            .Include(p => p.DeliveryAddresses)
            .FirstOrDefaultAsync(p => p.Id == request.PartnerId, ct)
            ?? throw new KeyNotFoundException($"Partner {request.PartnerId} not found.");

        var address = partner.DeliveryAddresses?.FirstOrDefault(a => a.Id == request.AddressId)
            ?? throw new KeyNotFoundException($"Address {request.AddressId} not found on partner.");

        partner.DeliveryAddresses!.Remove(address);
        await db.SaveChangesAsync(ct);
    }
}

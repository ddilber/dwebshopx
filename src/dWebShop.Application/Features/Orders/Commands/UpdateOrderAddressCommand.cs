using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Partners;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Orders.Commands;

public record UpdateOrderAddressCommand(
    int OrderId,
    string Address1,
    string Address2,
    string ZipCode,
    string City,
    string Country) : IRequest;

public class UpdateOrderAddressCommandHandler(IAppDbContext db)
    : IRequestHandler<UpdateOrderAddressCommand>
{
    public async Task Handle(UpdateOrderAddressCommand request, CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.DeliveryAddress)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
            ?? throw new InvalidOperationException("Order not found.");

        if (order.DeliveryAddress is not null)
        {
            order.DeliveryAddress.Address1 = request.Address1;
            order.DeliveryAddress.Address2 = request.Address2;
            order.DeliveryAddress.ZipCode  = request.ZipCode;
            order.DeliveryAddress.City     = request.City;
            order.DeliveryAddress.Country  = request.Country;
        }
        else
        {
            var address = new Address
            {
                Address1 = request.Address1,
                Address2 = request.Address2,
                ZipCode  = request.ZipCode,
                City     = request.City,
                Country  = request.Country,
            };
            db.Addresses.Add(address);
            await db.SaveChangesAsync(ct);
            order.DeliveryAddressId = address.Id;
        }

        await db.SaveChangesAsync(ct);
    }
}

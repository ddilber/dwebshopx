using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Orders.Commands;

public record AdminOrderLineItem(
    int ProductId,
    int SkuId,
    decimal Quantity,
    decimal Price,
    decimal Tax,
    decimal Discount = 0);

public record AdminCreateOrderCommand(
    int PartnerId,
    List<AdminOrderLineItem> Items,
    int? DeliveryAddressId = null,
    string? Notes = null,
    string Channel = "Web") : IRequest<Guid>;

public class AdminCreateOrderCommandHandler(IAppDbContext db)
    : IRequestHandler<AdminCreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(AdminCreateOrderCommand request, CancellationToken ct)
    {
        if (request.Items.Count == 0)
            throw new InvalidOperationException("Order must have at least one item.");

        var partnerExists = await db.Partners.AnyAsync(p => p.Id == request.PartnerId, ct);
        if (!partnerExists)
            throw new InvalidOperationException("Partner not found.");

        var orderGuid = Guid.NewGuid();
        var order = new Order
        {
            PartnerId = request.PartnerId,
            Guid = orderGuid,
            Status = OrderStatus.Pending,
            DeliveryAddressId = request.DeliveryAddressId,
            Notes = request.Notes,
            Channel = request.Channel,
            PaymentStatus = "Pending",
            Created = DateTime.UtcNow,
            Items = request.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                SkuId = i.SkuId,
                Quantity = i.Quantity,
                Price = i.Price,
                Tax = i.Tax,
                Discount = i.Discount,
            }).ToList()
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
        return orderGuid;
    }
}

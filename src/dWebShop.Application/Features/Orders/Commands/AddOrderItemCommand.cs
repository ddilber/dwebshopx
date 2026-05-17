using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Orders.Commands;

public record AddOrderItemCommand(
    int OrderId,
    int ProductId,
    int SkuId,
    decimal Quantity,
    decimal Price) : IRequest;

public class AddOrderItemCommandHandler(IAppDbContext db)
    : IRequestHandler<AddOrderItemCommand>
{
    public async Task Handle(AddOrderItemCommand request, CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
            ?? throw new InvalidOperationException("Order not found.");

        var sku = await db.ProductSkus.FindAsync([request.SkuId], ct)
            ?? throw new InvalidOperationException("SKU not found.");

        (order.Items ??= new List<OrderItem>()).Add(new OrderItem
        {
            ProductId = request.ProductId,
            SkuId     = request.SkuId,
            Quantity  = request.Quantity,
            Price     = request.Price,
            Tax       = sku.Tax,
            Discount  = 0,
        });

        await db.SaveChangesAsync(ct);
    }
}

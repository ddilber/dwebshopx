using dWebShop.Application.Common.Interfaces;
using dWebShop.Application.Services;
using dWebShop.Domain.Entities.Orders;
using dWebShop.Domain.Entities.Partners;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace dWebShop.Application.Features.Orders.Commands;

public record PlaceOrderCommand(
    int UserId,
    int PartnerId,
    int? DeliveryAddressId,
    string? Notes) : IRequest<Guid>;

public class PlaceOrderCommandHandler(
    IAppDbContext db,
    IEmailService emailService,
    IConfiguration configuration) : IRequestHandler<PlaceOrderCommand, Guid>
{
    public async Task<Guid> Handle(PlaceOrderCommand request, CancellationToken ct)
    {
        var cartItems = await db.ShoppingCartItems
            .Where(x => x.UserId == request.UserId)
            .ToListAsync(ct);

        if (cartItems.Count == 0)
            throw new InvalidOperationException("Cart is empty.");

        var partner = await db.Partners
            .Include(p => p.Address)
            .Include(p => p.DeliveryAddresses)
            .FirstOrDefaultAsync(p => p.Id == request.PartnerId, ct)
            ?? throw new InvalidOperationException("Partner not found.");

        var orderGuid = Guid.NewGuid();
        var order = new Order
        {
            PartnerId = request.PartnerId,
            Guid = orderGuid,
            Status = OrderStatus.Pending,
            DeliveryAddressId = request.DeliveryAddressId,
            Notes = request.Notes,
            Created = DateTime.UtcNow,
            Items = cartItems.Select(ci => new OrderItem
            {
                ProductId = ci.ProductId,
                SkuId = ci.SkuId,
                Quantity = ci.Quantity,
                Price = ci.Price,
                Tax = ci.Tax,
                Discount = ci.Discount,
            }).ToList()
        };

        db.Orders.Add(order);
        db.ShoppingCartItems.RemoveRange(cartItems);
        await db.SaveChangesAsync(ct);

        // Send confirmation emails (fire-and-forget style — don't fail the order on email errors)
        try
        {
            var adminEmail = configuration["Seed:AdminEmail"] ?? "admin@dwebshop.local";
            var clientName = $"{partner.FirstName} {partner.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(clientName)) clientName = partner.CompanyName;

            var emailItems = cartItems.Select(ci => new OrderEmailItem(ci.Name, ci.SKU, ci.Quantity, ci.Price));
            var total = cartItems.Sum(ci => ci.Price * ci.Quantity);

            Address? deliveryAddr = null;
            if (request.DeliveryAddressId.HasValue)
                deliveryAddr = await db.Addresses.FindAsync(new object[] { request.DeliveryAddressId.Value }, ct);
            if (deliveryAddr is null)
                deliveryAddr = partner.Address;

            var deliveryAddrStr = deliveryAddr is null
                ? "—"
                : $"{deliveryAddr.Address1}, {deliveryAddr.ZipCode} {deliveryAddr.City}, {deliveryAddr.Country}";

            await emailService.SendOrderConfirmationToClientAsync(partner.Email, clientName, orderGuid, emailItems, total, deliveryAddrStr);
            await emailService.SendOrderConfirmationToAdminAsync(adminEmail, clientName, partner.Email, orderGuid, emailItems, total);
        }
        catch
        {
            // Email failure should not roll back the order
        }

        return orderGuid;
    }
}

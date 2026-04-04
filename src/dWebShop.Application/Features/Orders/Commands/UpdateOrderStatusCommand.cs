using dWebShop.Application.Common.Interfaces;
using dWebShop.Application.Services;
using dWebShop.Domain.Entities.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Orders.Commands;

public record UpdateOrderStatusCommand(int OrderId, OrderStatus NewStatus) : IRequest;

public class UpdateOrderStatusCommandHandler(
    IAppDbContext db,
    IEmailService emailService) : IRequestHandler<UpdateOrderStatusCommand>
{
    public async Task Handle(UpdateOrderStatusCommand request, CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.Partner)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);

        if (order is null) return;

        order.Status = request.NewStatus;
        await db.SaveChangesAsync(ct);

        try
        {
            if (order.Partner is not null)
            {
                var clientName = $"{order.Partner.FirstName} {order.Partner.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(clientName)) clientName = order.Partner.CompanyName;
                await emailService.SendOrderStatusChangedToClientAsync(
                    order.Partner.Email, clientName, order.Guid, request.NewStatus.ToString());
            }
        }
        catch
        {
            // Email failure should not throw
        }
    }
}

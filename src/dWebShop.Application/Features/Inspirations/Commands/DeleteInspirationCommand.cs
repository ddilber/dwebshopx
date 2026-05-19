using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Inspirations.Commands;

public record DeleteInspirationCommand(int Id) : IRequest;

public class DeleteInspirationCommandHandler(IAppDbContext db) : IRequestHandler<DeleteInspirationCommand>
{
    public async Task Handle(DeleteInspirationCommand request, CancellationToken ct)
    {
        var inspiration = await db.Inspirations.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Inspiration {request.Id} not found.");

        db.Inspirations.Remove(inspiration);
        await db.SaveChangesAsync(ct);
    }
}

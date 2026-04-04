using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace dWebShop.Application.Features.Brands.Commands;

public record DeleteBrandCommand(int Id) : IRequest;

public class DeleteBrandCommandHandler(IAppDbContext db, IMemoryCache cache) : IRequestHandler<DeleteBrandCommand>
{
    public async Task Handle(DeleteBrandCommand request, CancellationToken ct)
    {
        var brand = await db.Brands.FirstOrDefaultAsync(b => b.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Brand {request.Id} not found.");
        db.Brands.Remove(brand);
        await db.SaveChangesAsync(ct);
        cache.Remove("brands:all");
    }
}

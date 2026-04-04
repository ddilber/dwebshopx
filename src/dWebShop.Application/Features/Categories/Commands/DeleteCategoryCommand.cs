using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace dWebShop.Application.Features.Categories.Commands;

public record DeleteCategoryCommand(int Id) : IRequest;

public class DeleteCategoryCommandHandler(IAppDbContext db, IMemoryCache cache) : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Category {request.Id} not found.");
        var brandId = category.BrandId;
        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);
        cache.Remove("categories:all");
        if (brandId.HasValue) cache.Remove($"categories:{brandId}");
    }
}

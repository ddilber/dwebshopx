using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Products.Commands;

/// <summary>
/// Updates SortOrder for each image in the provided list.
/// Pass the images in the desired display order; their index becomes the new SortOrder.
/// </summary>
public record ReorderProductImagesCommand(IReadOnlyList<int> OrderedImageIds) : IRequest;

public class ReorderProductImagesCommandHandler(IAppDbContext db) : IRequestHandler<ReorderProductImagesCommand>
{
    public async Task Handle(ReorderProductImagesCommand request, CancellationToken ct)
    {
        var ids = request.OrderedImageIds;
        var images = await db.ProductImages
            .Where(i => ids.Contains(i.Id))
            .ToListAsync(ct);

        foreach (var image in images)
        {
            var idx = ids.ToList().IndexOf(image.Id);
            if (idx >= 0) image.SortOrder = idx;
        }

        await db.SaveChangesAsync(ct);
    }
}

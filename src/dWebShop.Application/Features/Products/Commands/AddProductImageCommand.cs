using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Products.Commands;

public record AddProductImageCommand(int ProductId, string Path, string Description) : IRequest<int>;

public class AddProductImageCommandHandler(IAppDbContext db) : IRequestHandler<AddProductImageCommand, int>
{
    public async Task<int> Handle(AddProductImageCommand request, CancellationToken ct)
    {
        var details = await db.ProductDetails.FirstOrDefaultAsync(pd => pd.ProductId == request.ProductId, ct);
        if (details is null)
        {
            details = new ProductDetails { ProductId = request.ProductId, DetailDescription = string.Empty };
            db.ProductDetails.Add(details);
            await db.SaveChangesAsync(ct);
        }

        var maxSort = await db.ProductImages
            .Where(i => i.ProductDetailsId == details.Id)
            .Select(i => (int?)i.SortOrder)
            .MaxAsync(ct) ?? 0;

        var image = new ProductImage { Path = request.Path, Description = request.Description, ProductDetailsId = details.Id, SortOrder = maxSort + 1 };
        db.ProductImages.Add(image);
        await db.SaveChangesAsync(ct);
        return image.Id;
    }
}

public record DeleteProductImageCommand(int ImageId) : IRequest;

public class DeleteProductImageCommandHandler(IAppDbContext db) : IRequestHandler<DeleteProductImageCommand>
{
    public async Task Handle(DeleteProductImageCommand request, CancellationToken ct)
    {
        var image = await db.ProductImages.FirstOrDefaultAsync(i => i.Id == request.ImageId, ct)
            ?? throw new KeyNotFoundException($"ProductImage {request.ImageId} not found.");
        db.ProductImages.Remove(image);
        await db.SaveChangesAsync(ct);
    }
}

using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Products.Commands;

public record AddProductDocumentCommand(int ProductId, string Name, string Path, string Description) : IRequest<int>;

public class AddProductDocumentCommandHandler(IAppDbContext db) : IRequestHandler<AddProductDocumentCommand, int>
{
    public async Task<int> Handle(AddProductDocumentCommand request, CancellationToken ct)
    {
        var details = await db.ProductDetails.FirstOrDefaultAsync(pd => pd.ProductId == request.ProductId, ct);
        if (details is null)
        {
            details = new ProductDetails { ProductId = request.ProductId, DetailDescription = string.Empty };
            db.ProductDetails.Add(details);
            await db.SaveChangesAsync(ct);
        }

        var doc = new ProductDocument { Name = request.Name, Path = request.Path, Description = request.Description, ProductDetailsId = details.Id };
        db.ProductDocuments.Add(doc);
        await db.SaveChangesAsync(ct);
        return doc.Id;
    }
}

public record DeleteProductDocumentCommand(int DocumentId) : IRequest;

public class DeleteProductDocumentCommandHandler(IAppDbContext db) : IRequestHandler<DeleteProductDocumentCommand>
{
    public async Task Handle(DeleteProductDocumentCommand request, CancellationToken ct)
    {
        var doc = await db.ProductDocuments.FirstOrDefaultAsync(d => d.Id == request.DocumentId, ct)
            ?? throw new KeyNotFoundException($"ProductDocument {request.DocumentId} not found.");
        db.ProductDocuments.Remove(doc);
        await db.SaveChangesAsync(ct);
    }
}

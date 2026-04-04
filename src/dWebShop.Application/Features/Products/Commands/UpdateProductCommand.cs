using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Products;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Products.Commands;

public record UpdateProductCommand(int Id, string Name, string SKU, string ExtRef, string Slug, string Description, bool IsActive, int? BrandId, string DetailDescription, List<int>? CategoryIds) : IRequest;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(300);
        RuleFor(x => x.SKU).MaximumLength(100);
    }
}

public class UpdateProductCommandHandler(IAppDbContext db) : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await db.Products
            .Include(p => p.ProductDetails)
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Product {request.Id} not found.");

        product.Name = request.Name;
        product.SKU = request.SKU;
        product.ExtRef = request.ExtRef;
        product.Slug = request.Slug;
        product.Description = request.Description;
        product.IsActive = request.IsActive;
        product.BrandId = request.BrandId;

        if (product.ProductDetails is null)
            product.ProductDetails = new ProductDetails { DetailDescription = request.DetailDescription };
        else
            product.ProductDetails.DetailDescription = request.DetailDescription;

        if (request.CategoryIds is not null)
        {
            var cats = db.Categories.Where(c => request.CategoryIds.Contains(c.Id)).ToList();
            product.Categories = cats;
        }

        await db.SaveChangesAsync(ct);
    }
}

using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Products;
using FluentValidation;
using MediatR;

namespace dWebShop.Application.Features.Products.Commands;

public record CreateProductCommand(string Name, string SKU, string ExtRef, string Slug, string Description, bool IsActive, int? BrandId, List<int>? CategoryIds) : IRequest<int>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(300);
        RuleFor(x => x.SKU).MaximumLength(100);
    }
}

public class CreateProductCommandHandler(IAppDbContext db) : IRequestHandler<CreateProductCommand, int>
{
    public async Task<int> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = new Product
        {
            Name = request.Name,
            SKU = request.SKU,
            ExtRef = request.ExtRef,
            Slug = request.Slug,
            Description = request.Description,
            IsActive = request.IsActive,
            BrandId = request.BrandId,
            ProductDetails = new ProductDetails { DetailDescription = string.Empty },
        };

        if (request.CategoryIds?.Count > 0)
        {
            var cats = db.Categories.Where(c => request.CategoryIds.Contains(c.Id)).ToList();
            product.Categories = cats;
        }

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        return product.Id;
    }
}

using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Products;
using FluentValidation;
using MediatR;

namespace dWebShop.Application.Features.Categories.Commands;

public record CreateCategoryCommand(string Name, string Slug, string Description, int? ParentCategoryId, int? BrandId) : IRequest<int>;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200).Matches("^[a-z0-9-]+$").WithMessage("Slug must be lowercase alphanumeric with hyphens.");
    }
}

public class CreateCategoryCommandHandler(IAppDbContext db) : IRequestHandler<CreateCategoryCommand, int>
{
    public async Task<int> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var category = new Category
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            CategoryId = request.ParentCategoryId,
            BrandId = request.BrandId,
        };
        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);
        return category.Id;
    }
}

using dWebShop.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Categories.Commands;

public record UpdateCategoryCommand(int Id, string Name, string Slug, string Description, int? ParentCategoryId, int? BrandId) : IRequest;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200).Matches("^[a-z0-9-]+$").WithMessage("Slug must be lowercase alphanumeric with hyphens.");
    }
}

public class UpdateCategoryCommandHandler(IAppDbContext db) : IRequestHandler<UpdateCategoryCommand>
{
    public async Task Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Category {request.Id} not found.");
        category.Name = request.Name;
        category.Slug = request.Slug;
        category.Description = request.Description;
        category.CategoryId = request.ParentCategoryId;
        category.BrandId = request.BrandId;
        await db.SaveChangesAsync(ct);
    }
}

using dWebShop.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Brands.Commands;

public record UpdateBrandCommand(int Id, string Name, string Slug, string Description, string LogoImage, string SliderImage) : IRequest;

public class UpdateBrandCommandValidator : AbstractValidator<UpdateBrandCommand>
{
    public UpdateBrandCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200).Matches("^[a-z0-9-]+$").WithMessage("Slug must be lowercase alphanumeric with hyphens.");
    }
}

public class UpdateBrandCommandHandler(IAppDbContext db) : IRequestHandler<UpdateBrandCommand>
{
    public async Task Handle(UpdateBrandCommand request, CancellationToken ct)
    {
        var brand = await db.Brands.FirstOrDefaultAsync(b => b.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Brand {request.Id} not found.");
        brand.Name = request.Name;
        brand.Slug = request.Slug;
        brand.Description = request.Description;
        if (!string.IsNullOrWhiteSpace(request.LogoImage)) brand.LogoImage = request.LogoImage;
        if (!string.IsNullOrWhiteSpace(request.SliderImage)) brand.SliderImage = request.SliderImage;
        await db.SaveChangesAsync(ct);
    }
}

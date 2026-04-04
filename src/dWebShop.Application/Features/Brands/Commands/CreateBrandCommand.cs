using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Products;
using FluentValidation;
using MediatR;

namespace dWebShop.Application.Features.Brands.Commands;

public record CreateBrandCommand(string Name, string Slug, string Description, string LogoImage, string SliderImage) : IRequest<int>;

public class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    public CreateBrandCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200).Matches("^[a-z0-9-]+$").WithMessage("Slug must be lowercase alphanumeric with hyphens.");
    }
}

public class CreateBrandCommandHandler(IAppDbContext db) : IRequestHandler<CreateBrandCommand, int>
{
    public async Task<int> Handle(CreateBrandCommand request, CancellationToken ct)
    {
        var brand = new Brand
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            LogoImage = request.LogoImage,
            SliderImage = request.SliderImage,
        };
        db.Brands.Add(brand);
        await db.SaveChangesAsync(ct);
        return brand.Id;
    }
}

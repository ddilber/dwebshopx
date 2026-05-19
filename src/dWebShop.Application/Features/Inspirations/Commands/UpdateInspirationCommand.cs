using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Inspirations;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Inspirations.Commands;

public record UpdateInspirationCommand(
    int Id,
    string Title,
    string Slug,
    string Lede,
    string HeroLabel,
    string PublishedAt,
    int ReadMin,
    string Authors,
    string Tags,
    string Content,
    string LinkedProductSlugs,
    InspirationContentType ContentType,
    bool IsFeatured,
    bool Published,
    string BrandSlug) : IRequest;

public class UpdateInspirationCommandValidator : AbstractValidator<UpdateInspirationCommand>
{
    public UpdateInspirationCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(400);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(400).Matches("^[a-z0-9-]+$")
            .WithMessage("Slug must be lowercase alphanumeric with hyphens.");
        RuleFor(x => x.BrandSlug).NotEmpty();
    }
}

public class UpdateInspirationCommandHandler(IAppDbContext db) : IRequestHandler<UpdateInspirationCommand>
{
    public async Task Handle(UpdateInspirationCommand request, CancellationToken ct)
    {
        var inspiration = await db.Inspirations
            .Include(x => x.Brand)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Inspiration {request.Id} not found.");

        var brand = await db.Brands.FirstOrDefaultAsync(b => b.Slug == request.BrandSlug, ct)
            ?? throw new KeyNotFoundException($"Brand '{request.BrandSlug}' not found.");

        inspiration.Title = request.Title;
        inspiration.Slug = request.Slug;
        inspiration.Lede = request.Lede;
        inspiration.HeroLabel = request.HeroLabel;
        inspiration.PublishedAt = request.PublishedAt;
        inspiration.ReadMin = request.ReadMin;
        inspiration.Authors = request.Authors;
        inspiration.Tags = request.Tags;
        inspiration.Content = request.Content;
        inspiration.LinkedProductSlugs = request.LinkedProductSlugs;
        inspiration.ContentType = request.ContentType;
        inspiration.IsFeatured = request.IsFeatured;
        inspiration.Published = request.Published;
        inspiration.BrandId = brand.Id;

        await db.SaveChangesAsync(ct);
    }
}

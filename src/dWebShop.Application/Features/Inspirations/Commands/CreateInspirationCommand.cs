using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Inspirations;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Inspirations.Commands;

public record CreateInspirationCommand(
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
    string BrandSlug,
    string CoverImage,
    string GalleryJson,
    string MetaTitle,
    string MetaDescription,
    string OgImage) : IRequest<int>;

public class CreateInspirationCommandValidator : AbstractValidator<CreateInspirationCommand>
{
    public CreateInspirationCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(400);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(400).Matches("^[a-z0-9-]+$")
            .WithMessage("Slug must be lowercase alphanumeric with hyphens.");
        RuleFor(x => x.BrandSlug).NotEmpty();
    }
}

public class CreateInspirationCommandHandler(IAppDbContext db) : IRequestHandler<CreateInspirationCommand, int>
{
    public async Task<int> Handle(CreateInspirationCommand request, CancellationToken ct)
    {
        var brand = await db.Brands.FirstOrDefaultAsync(b => b.Slug == request.BrandSlug, ct)
            ?? throw new KeyNotFoundException($"Brand '{request.BrandSlug}' not found.");

        var inspiration = new Inspiration
        {
            Title = request.Title,
            Slug = request.Slug,
            Lede = request.Lede,
            HeroLabel = request.HeroLabel,
            PublishedAt = request.PublishedAt,
            ReadMin = request.ReadMin,
            Authors = request.Authors,
            Tags = request.Tags,
            Content = request.Content,
            LinkedProductSlugs = request.LinkedProductSlugs,
            ContentType = request.ContentType,
            IsFeatured = request.IsFeatured,
            Published = request.Published,
            BrandId = brand.Id,
            CoverImage = request.CoverImage,
            GalleryJson = string.IsNullOrWhiteSpace(request.GalleryJson) ? "[]" : request.GalleryJson,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription,
            OgImage = request.OgImage,
        };

        db.Inspirations.Add(inspiration);
        await db.SaveChangesAsync(ct);
        return inspiration.Id;
    }
}

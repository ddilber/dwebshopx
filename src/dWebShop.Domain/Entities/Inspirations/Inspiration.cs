using dWebShop.Domain.Common;
using dWebShop.Domain.Entities.Products;

namespace dWebShop.Domain.Entities.Inspirations;

public class Inspiration : BaseAuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Lede { get; set; } = string.Empty;
    public string HeroLabel { get; set; } = string.Empty;
    public string PublishedAt { get; set; } = string.Empty;
    public int ReadMin { get; set; }
    public string Authors { get; set; } = string.Empty;         // pipe-separated
    public string Tags { get; set; } = string.Empty;            // pipe-separated
    public string Content { get; set; } = string.Empty;         // JSON: InspirationSectionDto[]
    public string LinkedProductSlugs { get; set; } = string.Empty; // pipe-separated
    public InspirationContentType ContentType { get; set; }
    public bool IsFeatured { get; set; }
    public bool Published { get; set; }
    public int? BrandId { get; set; }
    public Brand? Brand { get; set; }
}

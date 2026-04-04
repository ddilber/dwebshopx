using dWebShop.Domain.Common;

namespace dWebShop.Domain.Entities.Products;

public class Brand : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LogoImage { get; set; } = string.Empty;
    public string SliderImage { get; set; } = string.Empty;
    public ICollection<Product>? Products { get; set; }
    public ICollection<Category>? Categories { get; set; }
}

using dWebShop.Domain.Common;

namespace dWebShop.Domain.Entities.Products;

public class Tag : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public ICollection<Product>? Products { get; set; }
    public ICollection<Category>? Categories { get; set; }
}

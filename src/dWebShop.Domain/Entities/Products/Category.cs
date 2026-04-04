using dWebShop.Domain.Common;

namespace dWebShop.Domain.Entities.Products;

public class Category : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public int? BrandId { get; set; }
    public Brand? Brand { get; set; }
    public ICollection<Product>? Products { get; set; }
    public ICollection<Tag>? Tags { get; set; }
    public ICollection<Category>? Categories { get; set; }
}

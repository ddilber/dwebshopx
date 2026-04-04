using dWebShop.Domain.Common;

namespace dWebShop.Domain.Entities.Products;

public class Product : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string ExtRef { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int? BrandId { get; set; }
    public Brand? Brand { get; set; }
    public ProductDetails? ProductDetails { get; set; }
    public ICollection<ProductSku>? ProductSkus { get; set; }
    public ICollection<ProductOption>? ProductOptions { get; set; }
    public ICollection<Category>? Categories { get; set; }
    public ICollection<Tag>? Tags { get; set; }
}

public class ProductDetails : BaseAuditableEntity
{
    public int? ProductId { get; set; }
    public string DetailDescription { get; set; } = string.Empty;
    public ICollection<ProductInfo>? Information { get; set; }
    public ICollection<ProductImage>? Images { get; set; }
    public ICollection<ProductDocument>? Documents { get; set; }
}

public class ProductInfo : BaseAuditableEntity
{
    public string Key { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public int? ProductDetailsId { get; set; }
}

public class ProductImage : BaseAuditableEntity
{
    public string Path { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? ProductDetailsId { get; set; }
}

public class ProductDocument : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? ProductDetailsId { get; set; }
}

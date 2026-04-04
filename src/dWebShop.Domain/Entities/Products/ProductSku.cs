using dWebShop.Domain.Common;

namespace dWebShop.Domain.Entities.Products;

public class ProductSku : BaseAuditableEntity
{
    public string SKU { get; set; } = string.Empty;
    public string ExtRef { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Tax { get; set; }
    public decimal Price { get; set; }
    public string? ImagePath { get; set; }
    public int? ProductId { get; set; }
    public ICollection<SkuOptionValue>? SkuOptionValues { get; set; }
}

public class ProductOption : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsNamePart { get; set; }
    public int? ProductId { get; set; }
    public ICollection<SkuOptionValue>? SkuOptionValues { get; set; }
    public ICollection<ProductOptionValue>? ProductOptionValues { get; set; }
}

public class ProductOptionValue : BaseAuditableEntity
{
    public int? ProductId { get; set; }
    public int? ProductOptionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<SkuOptionValue>? SkuOptionValues { get; set; }
}

public class SkuOptionValue : BaseAuditableEntity
{
    public int? ProductId { get; set; }
    public int? ProductSkuId { get; set; }
    public int? ProductOptionsId { get; set; }
    public ProductOption? ProductOption { get; set; }
    public int? ProductOptionValueId { get; set; }
    public ProductOptionValue? ProductOptionValue { get; set; }
}

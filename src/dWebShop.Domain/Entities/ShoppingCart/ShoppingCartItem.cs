using dWebShop.Domain.Common;
using dWebShop.Domain.Entities.Products;

namespace dWebShop.Domain.Entities.ShoppingCart;

public class ShoppingCartItem : BaseAuditableEntity
{
    public int ProductId { get; set; }
    public string ProductSlug { get; set; } = string.Empty;
    public int SkuId { get; set; }
    public UoM? UoM { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal Price { get; set; }
    public string? ImagePath { get; set; }
}

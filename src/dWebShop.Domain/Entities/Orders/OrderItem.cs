using dWebShop.Domain.Common;
using dWebShop.Domain.Entities.Products;

namespace dWebShop.Domain.Entities.Orders;

public class OrderItem : BaseAuditableEntity
{
    public int SkuId { get; set; }
    public ProductSku? Sku { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }

    // Pricing snapshot — immutable record of what was calculated at order time
    public decimal BasePrice { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal VatRateSnapshot { get; set; }
    public string? AppliedRulesJson { get; set; }
}

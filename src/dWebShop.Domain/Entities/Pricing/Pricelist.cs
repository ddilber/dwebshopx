using dWebShop.Domain.Common;
using dWebShop.Domain.Entities.Products;

namespace dWebShop.Domain.Entities.Pricing;

public class Pricelist : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public ICollection<PricelistItem>? Items { get; set; }
    public ICollection<ClientPricelist>? ClientPricelists { get; set; }
}

public class PricelistItem : BaseAuditableEntity
{
    public int PricelistId { get; set; }
    public Pricelist? Pricelist { get; set; }
    public int? ProductSkuId { get; set; }
    public ProductSku? ProductSku { get; set; }
    public decimal Price { get; set; }
    public decimal? MinQuantity { get; set; }
    public string Currency { get; set; } = "EUR";
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}

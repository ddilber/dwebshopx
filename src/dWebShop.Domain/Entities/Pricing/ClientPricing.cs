using dWebShop.Domain.Common;
using dWebShop.Domain.Entities.Partners;

namespace dWebShop.Domain.Entities.Pricing;

public class ClientPricelist : BaseAuditableEntity
{
    public int PartnerId { get; set; }
    public Partner? Partner { get; set; }
    public int PricelistId { get; set; }
    public Pricelist? Pricelist { get; set; }
    public bool IsDefault { get; set; }
}

public class ClientDiscount : BaseAuditableEntity
{
    public int PartnerId { get; set; }
    public Partner? Partner { get; set; }
    public decimal DiscountPercent { get; set; }
    public string? Description { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}

using dWebShop.Domain.Common;
using dWebShop.Domain.Entities.Partners;
using dWebShop.Domain.Entities.Products;

namespace dWebShop.Domain.Entities.Pricing;

public class CustomerProductPrice : BaseAuditableEntity
{
    public int PartnerId { get; set; }
    public Partner? Partner { get; set; }
    public int ProductSkuId { get; set; }
    public ProductSku? ProductSku { get; set; }
    public decimal Price { get; set; }
    public decimal? MinQuantity { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}

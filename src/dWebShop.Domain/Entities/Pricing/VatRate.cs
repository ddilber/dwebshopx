using dWebShop.Domain.Common;

namespace dWebShop.Domain.Entities.Pricing;

public class VatRate : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
}

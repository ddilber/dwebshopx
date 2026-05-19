using dWebShop.Domain.Common;

namespace dWebShop.Domain.Entities.Pricing;

public class PaymentTerms : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public int DueDays { get; set; }
    public decimal? CashDiscountPercent { get; set; }
    public int? CashDiscountDays { get; set; }
    public bool IsActive { get; set; } = true;
}

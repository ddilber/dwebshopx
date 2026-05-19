using dWebShop.Domain.Common;

namespace dWebShop.Domain.Entities.Pricing;

public enum DiscountType
{
    Percentage = 0,
    FixedAmount = 1,
    FixedPrice = 2,
}

public enum DiscountTargetType
{
    Product = 0,
    ProductGroup = 1,
    Category = 2,
    Customer = 3,
    Order = 4,
    PaymentTerm = 5,
}

public class DiscountDefinition : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DiscountType Type { get; set; }
    public bool AllowStacking { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<DiscountVersion>? Versions { get; set; }
}

public class DiscountVersion : BaseAuditableEntity
{
    public int DiscountDefinitionId { get; set; }
    public DiscountDefinition? DiscountDefinition { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsPublished { get; set; }
    public ICollection<DiscountRule>? Rules { get; set; }
}

public class DiscountRule : BaseAuditableEntity
{
    public int DiscountVersionId { get; set; }
    public DiscountVersion? DiscountVersion { get; set; }
    public DiscountTargetType TargetType { get; set; }
    public int TargetId { get; set; }
    public decimal Value { get; set; }
    public decimal? MinQuantity { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public bool IsExclusive { get; set; }
}

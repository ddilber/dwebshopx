using dWebShop.Domain.Common;

namespace dWebShop.Domain.Entities.Products;

public class UoM : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
}

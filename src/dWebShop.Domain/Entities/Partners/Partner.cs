using dWebShop.Domain.Common;

namespace dWebShop.Domain.Entities.Partners;

public class Partner : BaseAuditableEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public Address? Address { get; set; }
    public List<Address>? DeliveryAddresses { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

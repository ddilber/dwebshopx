using dWebShop.Domain.Common;

namespace dWebShop.Domain.Entities.Partners;

public class Address : BaseAuditableEntity
{
    public string Address1 { get; set; } = string.Empty;
    public string Address2 { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public AddressType? AddressType { get; set; }
}

public class AddressType : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
}

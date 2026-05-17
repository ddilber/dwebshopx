using dWebShop.Domain.Common;
using dWebShop.Domain.Entities.Partners;

namespace dWebShop.Domain.Entities.Orders;

public class Order : BaseAuditableEntity
{
    public int PartnerId { get; set; }
    public Guid Guid { get; set; }
    public Partner? Partner { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public int? DeliveryAddressId { get; set; }
    public Address? DeliveryAddress { get; set; }
    public string? Notes { get; set; }
    public string Channel { get; set; } = "Web";
    public string PaymentStatus { get; set; } = "Paid";
    public DateTime Created { get; set; }
    public DateTime Accepted { get; set; }
    public DateTime Delivered { get; set; }
    public ICollection<OrderItem>? Items { get; set; }
}

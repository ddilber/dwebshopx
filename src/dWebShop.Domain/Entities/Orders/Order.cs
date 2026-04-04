using dWebShop.Domain.Common;
using dWebShop.Domain.Entities.Partners;

namespace dWebShop.Domain.Entities.Orders;

public class Order : BaseAuditableEntity
{
    public int PartnerId { get; set; }
    public Guid Guid { get; set; }
    public Partner? Partner { get; set; }
    public DateTime Created { get; set; }
    public DateTime Accepted { get; set; }
    public DateTime Delivered { get; set; }
    public ICollection<OrderItem>? Items { get; set; }
}

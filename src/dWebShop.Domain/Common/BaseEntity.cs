using dWebShop.Domain.Common.Interfaces;

namespace dWebShop.Domain.Common;

public abstract class BaseEntity : IEntity
{
    public int Id { get; set; }
}

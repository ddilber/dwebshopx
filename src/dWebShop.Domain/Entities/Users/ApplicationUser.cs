using dWebShop.Domain.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace dWebShop.Domain.Entities.Users;

public class ApplicationUser : IdentityUser<int>, IEntity
{
    public bool IsApproved { get; set; }
    public int? PartnerId { get; set; }
}

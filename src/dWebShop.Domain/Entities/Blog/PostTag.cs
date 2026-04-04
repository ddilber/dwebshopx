using dWebShop.Domain.Common;

namespace dWebShop.Domain.Entities.Blog;

public class PostTag : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string MetaName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public ICollection<Post>? Posts { get; set; }
}

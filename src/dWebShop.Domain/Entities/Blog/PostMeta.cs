using dWebShop.Domain.Common;

namespace dWebShop.Domain.Entities.Blog;

public class PostMeta : BaseAuditableEntity
{
    public Post? Post { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

using dWebShop.Domain.Common;

namespace dWebShop.Domain.Entities.Blog;

public class PostComment : BaseAuditableEntity
{
    public Post? Post { get; set; }
    public string Title { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool Published { get; set; }
}

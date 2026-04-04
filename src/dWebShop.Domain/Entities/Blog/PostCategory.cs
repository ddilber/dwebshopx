using dWebShop.Domain.Common;

namespace dWebShop.Domain.Entities.Blog;

public class PostCategory : BaseAuditableEntity
{
    public int? ParentCategoryId { get; set; }
    public PostCategory? ParentCategory { get; set; }
    public ICollection<PostCategory>? Categories { get; set; }
    public ICollection<Post>? Posts { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MetaName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

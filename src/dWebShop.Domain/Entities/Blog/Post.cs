using dWebShop.Domain.Common;
using dWebShop.Domain.Entities.Users;

namespace dWebShop.Domain.Entities.Blog;

public class Post : BaseAuditableEntity
{
    public Post? Parent { get; set; }
    public ApplicationUser? User { get; set; }
    public ICollection<PostCategory>? Categories { get; set; }
    public ICollection<PostTag>? Tags { get; set; }
    public ICollection<PostMeta>? Metas { get; set; }
    public ICollection<PostComment>? Comments { get; set; }
    public int CommentCount { get; set; }
    public string Title { get; set; } = string.Empty;
    public string MetaTitle { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public bool Published { get; set; }
    public string Content { get; set; } = string.Empty;
}

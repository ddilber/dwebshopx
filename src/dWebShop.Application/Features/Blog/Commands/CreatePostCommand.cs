using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Blog;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Blog.Commands;

public record CreatePostCommand(
    string Title,
    string MetaTitle,
    string Slug,
    string Summary,
    string Content,
    bool Published,
    string MetaDescription,
    List<int> CategoryIds,
    List<int> TagIds) : IRequest<int>;

public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(300).Matches("^[a-z0-9-]+$")
            .WithMessage("Slug must be lowercase alphanumeric with hyphens.");
    }
}

public class CreatePostCommandHandler(IAppDbContext db) : IRequestHandler<CreatePostCommand, int>
{
    public async Task<int> Handle(CreatePostCommand request, CancellationToken ct)
    {
        var categories = await db.PostCategories.Where(c => request.CategoryIds.Contains(c.Id)).ToListAsync(ct);
        var tags = await db.PostTags.Where(t => request.TagIds.Contains(t.Id)).ToListAsync(ct);

        var post = new Post
        {
            Title = request.Title,
            MetaTitle = request.MetaTitle,
            Slug = request.Slug,
            Summary = request.Summary,
            Content = request.Content,
            Published = request.Published,
            Categories = categories,
            Tags = tags,
        };

        if (!string.IsNullOrWhiteSpace(request.MetaDescription))
        {
            post.Metas = [new PostMeta { Key = "description", Content = request.MetaDescription }];
        }

        db.Posts.Add(post);
        await db.SaveChangesAsync(ct);
        return post.Id;
    }
}

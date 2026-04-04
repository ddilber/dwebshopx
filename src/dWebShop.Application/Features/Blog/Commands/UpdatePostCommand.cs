using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Blog;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Blog.Commands;

public record UpdatePostCommand(
    int Id,
    string Title,
    string MetaTitle,
    string Slug,
    string Summary,
    string Content,
    bool Published,
    string MetaDescription,
    List<int> CategoryIds,
    List<int> TagIds) : IRequest;

public class UpdatePostCommandValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(300).Matches("^[a-z0-9-]+$")
            .WithMessage("Slug must be lowercase alphanumeric with hyphens.");
    }
}

public class UpdatePostCommandHandler(IAppDbContext db) : IRequestHandler<UpdatePostCommand>
{
    public async Task Handle(UpdatePostCommand request, CancellationToken ct)
    {
        var post = await db.Posts
            .Include(p => p.Categories)
            .Include(p => p.Tags)
            .Include(p => p.Metas)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Post {request.Id} not found.");

        post.Title = request.Title;
        post.MetaTitle = request.MetaTitle;
        post.Slug = request.Slug;
        post.Summary = request.Summary;
        post.Content = request.Content;
        post.Published = request.Published;

        var categories = await db.PostCategories.Where(c => request.CategoryIds.Contains(c.Id)).ToListAsync(ct);
        post.Categories = categories;

        var tags = await db.PostTags.Where(t => request.TagIds.Contains(t.Id)).ToListAsync(ct);
        post.Tags = tags;

        var metaMeta = post.Metas?.FirstOrDefault(m => m.Key == "description");
        if (!string.IsNullOrWhiteSpace(request.MetaDescription))
        {
            if (metaMeta is not null)
                metaMeta.Content = request.MetaDescription;
            else
            {
                post.Metas ??= [];
                post.Metas.Add(new PostMeta { Key = "description", Content = request.MetaDescription });
            }
        }
        else if (metaMeta is not null)
        {
            post.Metas!.Remove(metaMeta);
        }

        await db.SaveChangesAsync(ct);
    }
}

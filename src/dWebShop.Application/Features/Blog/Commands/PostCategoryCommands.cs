using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Blog;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Blog.Commands;

public record CreatePostCategoryCommand(string Name, string Slug) : IRequest<int>;

public class CreatePostCategoryCommandValidator : AbstractValidator<CreatePostCategoryCommand>
{
    public CreatePostCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200).Matches("^[a-z0-9-]+$")
            .WithMessage("Slug must be lowercase alphanumeric with hyphens.");
    }
}

public class CreatePostCategoryCommandHandler(IAppDbContext db) : IRequestHandler<CreatePostCategoryCommand, int>
{
    public async Task<int> Handle(CreatePostCategoryCommand request, CancellationToken ct)
    {
        var cat = new PostCategory { Name = request.Name, Slug = request.Slug };
        db.PostCategories.Add(cat);
        await db.SaveChangesAsync(ct);
        return cat.Id;
    }
}

public record DeletePostCategoryCommand(int Id) : IRequest;

public class DeletePostCategoryCommandHandler(IAppDbContext db) : IRequestHandler<DeletePostCategoryCommand>
{
    public async Task Handle(DeletePostCategoryCommand request, CancellationToken ct)
    {
        var cat = await db.PostCategories.FirstOrDefaultAsync(c => c.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"PostCategory {request.Id} not found.");
        db.PostCategories.Remove(cat);
        await db.SaveChangesAsync(ct);
    }
}

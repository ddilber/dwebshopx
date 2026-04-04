using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Blog;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Blog.Commands;

public record CreatePostTagCommand(string Name, string Slug) : IRequest<int>;

public class CreatePostTagCommandValidator : AbstractValidator<CreatePostTagCommand>
{
    public CreatePostTagCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100).Matches("^[a-z0-9-]+$")
            .WithMessage("Slug must be lowercase alphanumeric with hyphens.");
    }
}

public class CreatePostTagCommandHandler(IAppDbContext db) : IRequestHandler<CreatePostTagCommand, int>
{
    public async Task<int> Handle(CreatePostTagCommand request, CancellationToken ct)
    {
        var tag = new PostTag { Name = request.Name, Slug = request.Slug };
        db.PostTags.Add(tag);
        await db.SaveChangesAsync(ct);
        return tag.Id;
    }
}

public record DeletePostTagCommand(int Id) : IRequest;

public class DeletePostTagCommandHandler(IAppDbContext db) : IRequestHandler<DeletePostTagCommand>
{
    public async Task Handle(DeletePostTagCommand request, CancellationToken ct)
    {
        var tag = await db.PostTags.FirstOrDefaultAsync(t => t.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"PostTag {request.Id} not found.");
        db.PostTags.Remove(tag);
        await db.SaveChangesAsync(ct);
    }
}

using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Blog.Commands;

public record TogglePostPublishedCommand(int Id) : IRequest<bool>;

public class TogglePostPublishedCommandHandler(IAppDbContext db) : IRequestHandler<TogglePostPublishedCommand, bool>
{
    public async Task<bool> Handle(TogglePostPublishedCommand request, CancellationToken ct)
    {
        var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Post {request.Id} not found.");
        post.Published = !post.Published;
        await db.SaveChangesAsync(ct);
        return post.Published;
    }
}

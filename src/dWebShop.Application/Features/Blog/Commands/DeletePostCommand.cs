using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Blog.Commands;

public record DeletePostCommand(int Id) : IRequest;

public class DeletePostCommandHandler(IAppDbContext db) : IRequestHandler<DeletePostCommand>
{
    public async Task Handle(DeletePostCommand request, CancellationToken ct)
    {
        var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Post {request.Id} not found.");
        db.Posts.Remove(post);
        await db.SaveChangesAsync(ct);
    }
}

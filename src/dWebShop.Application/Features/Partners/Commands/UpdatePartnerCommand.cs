using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Partners.Commands;

public record UpdatePartnerCommand(
    int Id,
    string FirstName,
    string LastName,
    string CompanyName,
    string Email,
    string Phone) : IRequest;

public class UpdatePartnerCommandHandler(IAppDbContext db) : IRequestHandler<UpdatePartnerCommand>
{
    public async Task Handle(UpdatePartnerCommand request, CancellationToken ct)
    {
        var partner = await db.Partners.FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new InvalidOperationException($"Partner {request.Id} not found.");

        partner.FirstName = request.FirstName;
        partner.LastName = request.LastName;
        partner.CompanyName = request.CompanyName;
        partner.Email = request.Email;
        partner.Phone = request.Phone;

        await db.SaveChangesAsync(ct);
    }
}

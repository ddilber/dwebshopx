using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Partners;
using MediatR;

namespace dWebShop.Application.Features.Partners.Commands;

public record CreatePartnerCommand(
    string FirstName,
    string LastName,
    string CompanyName,
    string Email,
    string Phone) : IRequest<int>;

public class CreatePartnerCommandHandler(IAppDbContext db) : IRequestHandler<CreatePartnerCommand, int>
{
    public async Task<int> Handle(CreatePartnerCommand request, CancellationToken ct)
    {
        var partner = new Partner
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            CompanyName = request.CompanyName,
            Email = request.Email,
            Phone = request.Phone
        };

        db.Partners.Add(partner);
        await db.SaveChangesAsync(ct);
        return partner.Id;
    }
}

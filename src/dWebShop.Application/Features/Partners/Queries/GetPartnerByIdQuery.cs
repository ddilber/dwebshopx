using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Partners.Queries;

public record PartnerDetailDto(
    int Id,
    string FirstName,
    string LastName,
    string CompanyName,
    string Email,
    string Phone);

public record GetPartnerByIdQuery(int Id) : IRequest<PartnerDetailDto?>;

public class GetPartnerByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetPartnerByIdQuery, PartnerDetailDto?>
{
    public async Task<PartnerDetailDto?> Handle(GetPartnerByIdQuery request, CancellationToken ct) =>
        await db.Partners
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new PartnerDetailDto(p.Id, p.FirstName, p.LastName, p.CompanyName, p.Email, p.Phone))
            .FirstOrDefaultAsync(ct);
}

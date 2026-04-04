using dWebShop.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Profile.Commands;

public record UpdateContactDetailsCommand(
    int PartnerId,
    string FirstName,
    string LastName,
    string Email,
    string Phone) : IRequest;

public class UpdateContactDetailsCommandValidator : AbstractValidator<UpdateContactDetailsCommand>
{
    public UpdateContactDetailsCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(50);
    }
}

public class UpdateContactDetailsCommandHandler(IAppDbContext db) : IRequestHandler<UpdateContactDetailsCommand>
{
    public async Task Handle(UpdateContactDetailsCommand request, CancellationToken ct)
    {
        var partner = await db.Partners.FirstOrDefaultAsync(p => p.Id == request.PartnerId, ct)
            ?? throw new KeyNotFoundException($"Partner {request.PartnerId} not found.");

        partner.FirstName = request.FirstName;
        partner.LastName = request.LastName;
        partner.Email = request.Email;
        partner.Phone = request.Phone;

        await db.SaveChangesAsync(ct);
    }
}

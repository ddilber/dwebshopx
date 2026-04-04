using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Partners;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Profile.Commands;

public record AddDeliveryAddressCommand(
    int PartnerId,
    string Label,
    string Address1,
    string ZipCode,
    string City,
    string Country) : IRequest<int>;

public class AddDeliveryAddressCommandValidator : AbstractValidator<AddDeliveryAddressCommand>
{
    public AddDeliveryAddressCommandValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address1).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ZipCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
    }
}

public class AddDeliveryAddressCommandHandler(IAppDbContext db) : IRequestHandler<AddDeliveryAddressCommand, int>
{
    public async Task<int> Handle(AddDeliveryAddressCommand request, CancellationToken ct)
    {
        var partner = await db.Partners
            .Include(p => p.DeliveryAddresses)
            .FirstOrDefaultAsync(p => p.Id == request.PartnerId, ct)
            ?? throw new KeyNotFoundException($"Partner {request.PartnerId} not found.");

        var address = new Address
        {
            Label = request.Label,
            Address1 = request.Address1,
            ZipCode = request.ZipCode,
            City = request.City,
            Country = request.Country,
        };

        partner.DeliveryAddresses ??= [];
        partner.DeliveryAddresses.Add(address);

        await db.SaveChangesAsync(ct);
        return address.Id;
    }
}

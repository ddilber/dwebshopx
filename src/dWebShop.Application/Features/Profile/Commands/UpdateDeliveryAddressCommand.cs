using dWebShop.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Profile.Commands;

public record UpdateDeliveryAddressCommand(
    int AddressId,
    string Label,
    string Address1,
    string ZipCode,
    string City,
    string Country) : IRequest;

public class UpdateDeliveryAddressCommandValidator : AbstractValidator<UpdateDeliveryAddressCommand>
{
    public UpdateDeliveryAddressCommandValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address1).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ZipCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
    }
}

public class UpdateDeliveryAddressCommandHandler(IAppDbContext db) : IRequestHandler<UpdateDeliveryAddressCommand>
{
    public async Task Handle(UpdateDeliveryAddressCommand request, CancellationToken ct)
    {
        var address = await db.Addresses.FirstOrDefaultAsync(a => a.Id == request.AddressId, ct)
            ?? throw new KeyNotFoundException($"Address {request.AddressId} not found.");

        address.Label = request.Label;
        address.Address1 = request.Address1;
        address.ZipCode = request.ZipCode;
        address.City = request.City;
        address.Country = request.Country;

        await db.SaveChangesAsync(ct);
    }
}

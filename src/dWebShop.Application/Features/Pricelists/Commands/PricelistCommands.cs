using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Pricing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Pricelists.Commands;

// ---- Pricelist CRUD ----

public record CreatePricelistCommand(string Name, string Description, bool IsActive) : IRequest<int>;

public class CreatePricelistCommandHandler(IAppDbContext db) : IRequestHandler<CreatePricelistCommand, int>
{
    public async Task<int> Handle(CreatePricelistCommand request, CancellationToken ct)
    {
        var pl = new Pricelist { Name = request.Name, Description = request.Description, IsActive = request.IsActive };
        db.Pricelists.Add(pl);
        await db.SaveChangesAsync(ct);
        return pl.Id;
    }
}

public record UpdatePricelistCommand(int Id, string Name, string Description, bool IsActive) : IRequest;

public class UpdatePricelistCommandHandler(IAppDbContext db) : IRequestHandler<UpdatePricelistCommand>
{
    public async Task Handle(UpdatePricelistCommand request, CancellationToken ct)
    {
        var pl = await db.Pricelists.FindAsync([request.Id], ct)
            ?? throw new KeyNotFoundException($"Pricelist {request.Id} not found.");
        pl.Name = request.Name;
        pl.Description = request.Description;
        pl.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);
    }
}

public record DeletePricelistCommand(int Id) : IRequest;

public class DeletePricelistCommandHandler(IAppDbContext db) : IRequestHandler<DeletePricelistCommand>
{
    public async Task Handle(DeletePricelistCommand request, CancellationToken ct)
    {
        var pl = await db.Pricelists.FindAsync([request.Id], ct)
            ?? throw new KeyNotFoundException($"Pricelist {request.Id} not found.");
        db.Pricelists.Remove(pl);
        await db.SaveChangesAsync(ct);
    }
}

// ---- PricelistItem CRUD ----

public record UpsertPricelistItemCommand(int? Id, int PricelistId, int? ProductSkuId, decimal Price, decimal? MinQuantity) : IRequest<int>;

public class UpsertPricelistItemCommandHandler(IAppDbContext db) : IRequestHandler<UpsertPricelistItemCommand, int>
{
    public async Task<int> Handle(UpsertPricelistItemCommand request, CancellationToken ct)
    {
        PricelistItem item;
        if (request.Id.HasValue)
        {
            item = await db.PricelistItems.FindAsync([request.Id.Value], ct)
                ?? throw new KeyNotFoundException($"PricelistItem {request.Id} not found.");
            item.ProductSkuId = request.ProductSkuId;
            item.Price = request.Price;
            item.MinQuantity = request.MinQuantity;
        }
        else
        {
            item = new PricelistItem
            {
                PricelistId = request.PricelistId,
                ProductSkuId = request.ProductSkuId,
                Price = request.Price,
                MinQuantity = request.MinQuantity
            };
            db.PricelistItems.Add(item);
        }
        await db.SaveChangesAsync(ct);
        return item.Id;
    }
}

public record DeletePricelistItemCommand(int Id) : IRequest;

public class DeletePricelistItemCommandHandler(IAppDbContext db) : IRequestHandler<DeletePricelistItemCommand>
{
    public async Task Handle(DeletePricelistItemCommand request, CancellationToken ct)
    {
        var item = await db.PricelistItems.FindAsync([request.Id], ct)
            ?? throw new KeyNotFoundException($"PricelistItem {request.Id} not found.");
        db.PricelistItems.Remove(item);
        await db.SaveChangesAsync(ct);
    }
}

// ---- ClientPricelist assign/remove ----

public record AssignClientPricelistCommand(int PartnerId, int PricelistId, bool IsDefault) : IRequest<int>;

public class AssignClientPricelistCommandHandler(IAppDbContext db) : IRequestHandler<AssignClientPricelistCommand, int>
{
    public async Task<int> Handle(AssignClientPricelistCommand request, CancellationToken ct)
    {
        var existing = await db.ClientPricelists
            .FirstOrDefaultAsync(cp => cp.PartnerId == request.PartnerId && cp.PricelistId == request.PricelistId, ct);
        if (existing is not null)
        {
            existing.IsDefault = request.IsDefault;
            await db.SaveChangesAsync(ct);
            return existing.Id;
        }

        var cp = new ClientPricelist
        {
            PartnerId = request.PartnerId,
            PricelistId = request.PricelistId,
            IsDefault = request.IsDefault
        };
        db.ClientPricelists.Add(cp);
        await db.SaveChangesAsync(ct);
        return cp.Id;
    }
}

public record RemoveClientPricelistCommand(int Id) : IRequest;

public class RemoveClientPricelistCommandHandler(IAppDbContext db) : IRequestHandler<RemoveClientPricelistCommand>
{
    public async Task Handle(RemoveClientPricelistCommand request, CancellationToken ct)
    {
        var cp = await db.ClientPricelists.FindAsync([request.Id], ct)
            ?? throw new KeyNotFoundException($"ClientPricelist {request.Id} not found.");
        db.ClientPricelists.Remove(cp);
        await db.SaveChangesAsync(ct);
    }
}

// ---- ClientDiscount ----

public record UpsertClientDiscountCommand(int PartnerId, decimal DiscountPercent, string? Description, DateTime? ValidFrom, DateTime? ValidTo) : IRequest<int>;

public class UpsertClientDiscountCommandHandler(IAppDbContext db) : IRequestHandler<UpsertClientDiscountCommand, int>
{
    public async Task<int> Handle(UpsertClientDiscountCommand request, CancellationToken ct)
    {
        var existing = await db.ClientDiscounts
            .FirstOrDefaultAsync(d => d.PartnerId == request.PartnerId, ct);

        if (existing is not null)
        {
            existing.DiscountPercent = request.DiscountPercent;
            existing.Description = request.Description;
            existing.ValidFrom = request.ValidFrom;
            existing.ValidTo = request.ValidTo;
            await db.SaveChangesAsync(ct);
            return existing.Id;
        }

        var discount = new ClientDiscount
        {
            PartnerId = request.PartnerId,
            DiscountPercent = request.DiscountPercent,
            Description = request.Description,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo
        };
        db.ClientDiscounts.Add(discount);
        await db.SaveChangesAsync(ct);
        return discount.Id;
    }
}

public record DeleteClientDiscountCommand(int Id) : IRequest;

public class DeleteClientDiscountCommandHandler(IAppDbContext db) : IRequestHandler<DeleteClientDiscountCommand>
{
    public async Task Handle(DeleteClientDiscountCommand request, CancellationToken ct)
    {
        var d = await db.ClientDiscounts.FindAsync([request.Id], ct)
            ?? throw new KeyNotFoundException($"ClientDiscount {request.Id} not found.");
        db.ClientDiscounts.Remove(d);
        await db.SaveChangesAsync(ct);
    }
}

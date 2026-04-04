using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.ShoppingCart;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.ShoppingCart.Commands;

// Add or increment a SKU in the cart
public record AddToCartCommand(
    int UserId,
    int ProductId,
    string ProductSlug,
    int SkuId,
    string SKU,
    string Name,
    decimal Quantity,
    decimal Price,
    decimal Tax,
    string? ImagePath) : IRequest;

public class AddToCartCommandHandler(IAppDbContext db) : IRequestHandler<AddToCartCommand>
{
    public async Task Handle(AddToCartCommand request, CancellationToken ct)
    {
        var existing = await db.ShoppingCartItems
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && x.SkuId == request.SkuId, ct);

        if (existing is not null)
        {
            existing.Quantity += request.Quantity;
            existing.Price = request.Price;
        }
        else
        {
            db.ShoppingCartItems.Add(new ShoppingCartItem
            {
                UserId = request.UserId,
                ProductId = request.ProductId,
                ProductSlug = request.ProductSlug,
                SkuId = request.SkuId,
                SKU = request.SKU,
                Name = request.Name,
                Quantity = request.Quantity,
                Price = request.Price,
                Tax = request.Tax,
                ImagePath = request.ImagePath,
            });
        }

        await db.SaveChangesAsync(ct);
    }
}

// Update quantity of a specific cart item
public record UpdateCartItemCommand(int UserId, int CartItemId, decimal Quantity) : IRequest;

public class UpdateCartItemCommandHandler(IAppDbContext db) : IRequestHandler<UpdateCartItemCommand>
{
    public async Task Handle(UpdateCartItemCommand request, CancellationToken ct)
    {
        var item = await db.ShoppingCartItems
            .FirstOrDefaultAsync(x => x.Id == request.CartItemId && x.UserId == request.UserId, ct);

        if (item is null) return;

        item.Quantity = Math.Max(1, request.Quantity);
        await db.SaveChangesAsync(ct);
    }
}

// Remove a single item from the cart
public record RemoveCartItemCommand(int UserId, int CartItemId) : IRequest;

public class RemoveCartItemCommandHandler(IAppDbContext db) : IRequestHandler<RemoveCartItemCommand>
{
    public async Task Handle(RemoveCartItemCommand request, CancellationToken ct)
    {
        var item = await db.ShoppingCartItems
            .FirstOrDefaultAsync(x => x.Id == request.CartItemId && x.UserId == request.UserId, ct);

        if (item is null) return;

        db.ShoppingCartItems.Remove(item);
        await db.SaveChangesAsync(ct);
    }
}

// Clear all items in the cart for a user
public record ClearCartCommand(int UserId) : IRequest;

public class ClearCartCommandHandler(IAppDbContext db) : IRequestHandler<ClearCartCommand>
{
    public async Task Handle(ClearCartCommand request, CancellationToken ct)
    {
        var items = await db.ShoppingCartItems
            .Where(x => x.UserId == request.UserId)
            .ToListAsync(ct);

        db.ShoppingCartItems.RemoveRange(items);
        await db.SaveChangesAsync(ct);
    }
}

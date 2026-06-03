using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.ShoppingCart.Queries;

public record CartItemDto(
    int Id,
    int SkuId,
    string SKU,
    string Name,
    string ProductSlug,
    decimal Quantity,
    decimal Price,
    decimal Tax,
    decimal Discount,
    string? ImagePath,
    // Uom is denormalised from ProductSku at read time — cart doesn't store
    // its own copy. Falls back to empty if the SKU has been removed.
    string Uom);

public record GetCartQuery(int UserId) : IRequest<List<CartItemDto>>;

// Uses IAppDbContextFactory rather than the scoped IAppDbContext because the
// Navbar fires this query during static SSR layout render, concurrently with
// whatever the page's OnInitializedAsync is doing on the scoped DbContext.
// A fresh context per call keeps the two render paths from clashing on the
// shared connection.
public class GetCartQueryHandler(IAppDbContextFactory dbFactory) : IRequestHandler<GetCartQuery, List<CartItemDto>>
{
    public async Task<List<CartItemDto>> Handle(GetCartQuery request, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.ShoppingCartItems
            .AsNoTracking()
            .Where(x => x.UserId == request.UserId)
            .OrderBy(x => x.Name)
            .GroupJoin(
                db.ProductSkus.AsNoTracking(),
                cart => cart.SkuId,
                sku => sku.Id,
                (cart, skus) => new { cart, skus })
            .SelectMany(
                g => g.skus.DefaultIfEmpty(),
                (g, sku) => new CartItemDto(
                    g.cart.Id, g.cart.SkuId, g.cart.SKU, g.cart.Name, g.cart.ProductSlug,
                    g.cart.Quantity, g.cart.Price, g.cart.Tax, g.cart.Discount, g.cart.ImagePath,
                    sku != null ? sku.Uom : string.Empty))
            .ToListAsync(ct);
    }
}

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
    string? ImagePath);

public record GetCartQuery(int UserId) : IRequest<List<CartItemDto>>;

public class GetCartQueryHandler(IAppDbContext db) : IRequestHandler<GetCartQuery, List<CartItemDto>>
{
    public async Task<List<CartItemDto>> Handle(GetCartQuery request, CancellationToken ct) =>
        await db.ShoppingCartItems
            .AsNoTracking()
            .Where(x => x.UserId == request.UserId)
            .OrderBy(x => x.Name)
            .Select(x => new CartItemDto(
                x.Id, x.SkuId, x.SKU, x.Name, x.ProductSlug,
                x.Quantity, x.Price, x.Tax, x.Discount, x.ImagePath))
            .ToListAsync(ct);
}

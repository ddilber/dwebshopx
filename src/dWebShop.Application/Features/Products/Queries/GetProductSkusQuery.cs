using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Products.Queries;

public record OrderItemSkuDto(int Id, string SKU, string Name, decimal Price, int Stock);

public record GetSkusForOrderItemQuery(int ProductId) : IRequest<List<OrderItemSkuDto>>;

public class GetSkusForOrderItemQueryHandler(IAppDbContext db)
    : IRequestHandler<GetSkusForOrderItemQuery, List<OrderItemSkuDto>>
{
    public async Task<List<OrderItemSkuDto>> Handle(GetSkusForOrderItemQuery request, CancellationToken ct)
        => await db.ProductSkus
            .AsNoTracking()
            .Where(s => s.ProductId == request.ProductId)
            .OrderBy(s => s.Id)
            .Select(s => new OrderItemSkuDto(s.Id, s.SKU, s.Name, s.Price, s.StockQuantity))
            .ToListAsync(ct);
}

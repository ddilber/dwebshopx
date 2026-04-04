using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Orders.Queries;

// DTO types

public record OrderItemDto(
    int Id,
    int ProductId,
    string ProductName,
    int SkuId,
    string SKU,
    decimal Quantity,
    decimal Price,
    decimal Tax,
    decimal Discount);

public record OrderListDto(
    int Id,
    Guid Guid,
    OrderStatus Status,
    DateTime Created,
    int ItemCount,
    decimal Total);

public record OrderDetailDto(
    int Id,
    Guid Guid,
    OrderStatus Status,
    DateTime Created,
    string? Notes,
    string? DeliveryAddress,
    int PartnerId,
    string PartnerName,
    string PartnerEmail,
    List<OrderItemDto> Items);

// --- Portal: get orders for a partner ---

public record GetPartnerOrdersQuery(int PartnerId, int Page = 1, int PageSize = 10)
    : IRequest<(List<OrderListDto> Items, int TotalCount)>;

public class GetPartnerOrdersQueryHandler(IAppDbContext db)
    : IRequestHandler<GetPartnerOrdersQuery, (List<OrderListDto>, int)>
{
    public async Task<(List<OrderListDto>, int)> Handle(GetPartnerOrdersQuery request, CancellationToken ct)
    {
        var query = db.Orders
            .AsNoTracking()
            .Where(o => o.PartnerId == request.PartnerId)
            .OrderByDescending(o => o.Created);

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderListDto(
                o.Id,
                o.Guid,
                o.Status,
                o.Created,
                o.Items == null ? 0 : o.Items.Count,
                o.Items == null ? 0 : o.Items.Sum(i => i.Price * i.Quantity)))
            .ToListAsync(ct);

        return (items, total);
    }
}

// --- Portal/Admin: get single order by Guid ---

public record GetOrderByGuidQuery(Guid Guid, int? PartnerId = null) : IRequest<OrderDetailDto?>;

public class GetOrderByGuidQueryHandler(IAppDbContext db)
    : IRequestHandler<GetOrderByGuidQuery, OrderDetailDto?>
{
    public async Task<OrderDetailDto?> Handle(GetOrderByGuidQuery request, CancellationToken ct)
    {
        var query = db.Orders
            .AsNoTracking()
            .Include(o => o.Partner)
            .Include(o => o.DeliveryAddress)
            .Include(o => o.Items!)
                .ThenInclude(i => i.Product)
            .Include(o => o.Items!)
                .ThenInclude(i => i.Sku)
            .Where(o => o.Guid == request.Guid);

        if (request.PartnerId.HasValue)
            query = query.Where(o => o.PartnerId == request.PartnerId.Value);

        var order = await query.FirstOrDefaultAsync(ct);
        if (order is null) return null;

        var deliveryAddr = order.DeliveryAddress is null
            ? null
            : $"{order.DeliveryAddress.Address1}, {order.DeliveryAddress.ZipCode} {order.DeliveryAddress.City}, {order.DeliveryAddress.Country}";

        var partnerName = order.Partner is null ? "" :
            $"{order.Partner.FirstName} {order.Partner.LastName}".Trim();
        if (string.IsNullOrWhiteSpace(partnerName)) partnerName = order.Partner?.CompanyName ?? "";

        var itemDtos = (order.Items ?? [])
            .Select(i => new OrderItemDto(
                i.Id,
                i.ProductId,
                i.Product?.Name ?? "",
                i.SkuId,
                i.Sku?.SKU ?? "",
                i.Quantity,
                i.Price,
                i.Tax,
                i.Discount))
            .ToList();

        return new OrderDetailDto(
            order.Id,
            order.Guid,
            order.Status,
            order.Created,
            order.Notes,
            deliveryAddr,
            order.PartnerId,
            partnerName,
            order.Partner?.Email ?? "",
            itemDtos);
    }
}

// --- Admin: get all orders ---

public record GetAllOrdersQuery(int Page = 1, int PageSize = 20, string? Search = null)
    : IRequest<(List<AdminOrderListDto> Items, int TotalCount)>;

public record AdminOrderListDto(
    int Id,
    Guid Guid,
    OrderStatus Status,
    DateTime Created,
    string PartnerName,
    string PartnerEmail,
    int ItemCount,
    decimal Total);

public class GetAllOrdersQueryHandler(IAppDbContext db)
    : IRequestHandler<GetAllOrdersQuery, (List<AdminOrderListDto>, int)>
{
    public async Task<(List<AdminOrderListDto>, int)> Handle(GetAllOrdersQuery request, CancellationToken ct)
    {
        var query = db.Orders
            .AsNoTracking()
            .Include(o => o.Partner)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(o =>
                (o.Partner != null && (
                    o.Partner.FirstName.ToLower().Contains(s) ||
                    o.Partner.LastName.ToLower().Contains(s) ||
                    o.Partner.CompanyName.ToLower().Contains(s) ||
                    o.Partner.Email.ToLower().Contains(s))));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(o => o.Created)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new AdminOrderListDto(
                o.Id,
                o.Guid,
                o.Status,
                o.Created,
                (o.Partner == null ? "" : (o.Partner.FirstName + " " + o.Partner.LastName).Trim()),
                o.Partner == null ? "" : o.Partner.Email,
                o.Items == null ? 0 : o.Items.Count,
                o.Items == null ? 0 : o.Items.Sum(i => i.Price * i.Quantity)))
            .ToListAsync(ct);

        return (items, total);
    }
}

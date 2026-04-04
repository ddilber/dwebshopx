using dWebShop.Application.Common.Interfaces;
using dWebShop.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Infrastructure.Services;

public class PricingService(IAppDbContext db) : IPricingService
{
    public async Task<decimal?> ResolvePriceAsync(int? partnerId, int skuId, CancellationToken ct = default)
    {
        var prices = await ResolvePricesAsync(partnerId, [skuId], ct);
        return prices.GetValueOrDefault(skuId);
    }

    public async Task<Dictionary<int, decimal?>> ResolvePricesAsync(int? partnerId, IEnumerable<int> skuIds, CancellationToken ct = default)
    {
        var skuIdList = skuIds.ToList();

        // Load base prices for all requested SKUs
        var basePrices = await db.ProductSkus
            .Where(s => skuIdList.Contains(s.Id))
            .Select(s => new { s.Id, s.Price })
            .ToDictionaryAsync(s => s.Id, s => (decimal?)s.Price, ct);

        if (partnerId is null)
            return basePrices;

        // Load active client pricelist items for this partner
        var now = DateTime.UtcNow;
        var pricelistItems = await db.ClientPricelists
            .Where(cp => cp.PartnerId == partnerId)
            .Join(db.PricelistItems,
                cp => cp.PricelistId,
                pi => pi.PricelistId,
                (cp, pi) => new { pi.ProductSkuId, pi.Price })
            .Where(x => x.ProductSkuId != null && skuIdList.Contains(x.ProductSkuId!.Value))
            .ToListAsync(ct);

        // Load client discount (first active one)
        var discount = await db.ClientDiscounts
            .Where(d => d.PartnerId == partnerId &&
                        (d.ValidFrom == null || d.ValidFrom <= now) &&
                        (d.ValidTo == null || d.ValidTo >= now))
            .OrderByDescending(d => d.DiscountPercent)
            .FirstOrDefaultAsync(ct);

        var result = new Dictionary<int, decimal?>();

        foreach (var skuId in skuIdList)
        {
            // Step 1: check pricelist override
            var pricelistOverride = pricelistItems
                .Where(x => x.ProductSkuId == skuId)
                .Select(x => (decimal?)x.Price)
                .FirstOrDefault();

            var basePrice = basePrices.GetValueOrDefault(skuId);
            var resolvedPrice = pricelistOverride ?? basePrice;

            // Step 2: apply discount
            if (resolvedPrice.HasValue && discount is not null && discount.DiscountPercent > 0)
                resolvedPrice = resolvedPrice.Value * (1 - discount.DiscountPercent / 100m);

            result[skuId] = resolvedPrice;
        }

        return result;
    }
}

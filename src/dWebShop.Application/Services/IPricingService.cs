namespace dWebShop.Application.Services;

public interface IPricingService
{
    /// <summary>
    /// Resolves the final price for a given client and SKU.
    /// If partnerId is null, returns the base SKU price.
    /// Resolution order:
    ///   1. ClientPricelist → PricelistItem.Price
    ///   2. Base ProductSku.Price
    ///   3. Apply ClientDiscount (percent) if any
    /// </summary>
    Task<decimal?> ResolvePriceAsync(int? partnerId, int skuId, CancellationToken ct = default);

    /// <summary>
    /// Batch price resolution for multiple SKUs for a given client.
    /// Returns a dictionary of skuId → resolved price.
    /// </summary>
    Task<Dictionary<int, decimal?>> ResolvePricesAsync(int? partnerId, IEnumerable<int> skuIds, CancellationToken ct = default);
}

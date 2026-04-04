using dWebShop.Domain.Entities.Partners;
using dWebShop.Domain.Entities.Pricing;
using dWebShop.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Brand> Brands { get; }
    DbSet<Category> Categories { get; }
    DbSet<Tag> Tags { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductDetails> ProductDetails { get; }
    DbSet<ProductInfo> ProductInfos { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductDocument> ProductDocuments { get; }
    DbSet<ProductSku> ProductSkus { get; }
    DbSet<ProductOption> ProductOptions { get; }
    DbSet<ProductOptionValue> ProductOptionValues { get; }
    DbSet<SkuOptionValue> SkuOptionValues { get; }

    DbSet<Partner> Partners { get; }

    DbSet<Pricelist> Pricelists { get; }
    DbSet<PricelistItem> PricelistItems { get; }
    DbSet<ClientPricelist> ClientPricelists { get; }
    DbSet<ClientDiscount> ClientDiscounts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

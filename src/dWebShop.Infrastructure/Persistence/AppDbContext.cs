using dWebShop.Domain.Entities.Blog;
using dWebShop.Domain.Entities.Orders;
using dWebShop.Domain.Entities.Partners;
using dWebShop.Domain.Entities.Pricing;
using dWebShop.Domain.Entities.Products;
using dWebShop.Domain.Entities.ShoppingCart;
using dWebShop.Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>, dWebShop.Application.Common.Interfaces.IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Products
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<UoM> UoMs => Set<UoM>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductDetails> ProductDetails => Set<ProductDetails>();
    public DbSet<ProductInfo> ProductInfos => Set<ProductInfo>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductDocument> ProductDocuments => Set<ProductDocument>();
    public DbSet<ProductSku> ProductSkus => Set<ProductSku>();
    public DbSet<ProductOption> ProductOptions => Set<ProductOption>();
    public DbSet<ProductOptionValue> ProductOptionValues => Set<ProductOptionValue>();
    public DbSet<SkuOptionValue> SkuOptionValues => Set<SkuOptionValue>();

    // Orders
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    // Partners
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<AddressType> AddressTypes => Set<AddressType>();

    // Pricing
    public DbSet<Pricelist> Pricelists => Set<Pricelist>();
    public DbSet<PricelistItem> PricelistItems => Set<PricelistItem>();
    public DbSet<ClientPricelist> ClientPricelists => Set<ClientPricelist>();
    public DbSet<ClientDiscount> ClientDiscounts => Set<ClientDiscount>();

    // Shopping Cart
    public DbSet<ShoppingCartItem> ShoppingCartItems => Set<ShoppingCartItem>();

    // Blog
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostCategory> PostCategories => Set<PostCategory>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    public DbSet<PostMeta> PostMetas => Set<PostMeta>();
    public DbSet<PostComment> PostComments => Set<PostComment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Identity table renames
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<IdentityRole<int>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

        // Brand
        builder.Entity<Brand>(e =>
        {
            e.ToTable("Brands");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Slug).IsUnique();
        });

        // Category → Brand (many-to-one), Category self-ref
        builder.Entity<Category>(e =>
        {
            e.ToTable("Categories");
            e.HasOne(x => x.ParentCategory)
                .WithMany(x => x.Categories)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Brand)
                .WithMany(x => x.Categories)
                .HasForeignKey(x => x.BrandId)
                .OnDelete(DeleteBehavior.SetNull);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        });

        // Tag many-to-many with Product and Category
        builder.Entity<Tag>(e =>
        {
            e.ToTable("Tags");
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(100).IsRequired();
        });

        builder.Entity<Product>(e =>
        {
            e.ToTable("Products");
            e.HasOne(x => x.Brand)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.BrandId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Categories)
                .WithMany(x => x.Products);
            e.HasMany(x => x.Tags)
                .WithMany(x => x.Products)
                .UsingEntity("ProductTags");
            e.Property(x => x.Status).HasConversion<int>();
            e.Property(x => x.Name).HasMaxLength(300).IsRequired();
            e.Property(x => x.SKU).HasMaxLength(100);
            e.Property(x => x.Slug).HasMaxLength(300).IsRequired();
        });

        builder.Entity<ProductDetails>(e =>
        {
            e.ToTable("ProductDetails");
            e.HasOne<Product>()
                .WithOne(x => x.ProductDetails)
                .HasForeignKey<ProductDetails>(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductInfo>(e =>
        {
            e.ToTable("ProductInfos");
            e.HasOne<ProductDetails>()
                .WithMany(x => x.Information)
                .HasForeignKey(x => x.ProductDetailsId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductImage>(e =>
        {
            e.ToTable("ProductImages");
            e.HasOne<ProductDetails>()
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.ProductDetailsId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductDocument>(e =>
        {
            e.ToTable("ProductDocuments");
            e.HasOne<ProductDetails>()
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.ProductDetailsId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductSku>(e =>
        {
            e.ToTable("ProductSkus");
            e.HasOne<Product>()
                .WithMany(x => x.ProductSkus)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Gtin).HasMaxLength(14);
            e.Property(x => x.Price).HasPrecision(18, 4);
            e.Property(x => x.CompareAtPrice).HasPrecision(18, 4);
            e.Property(x => x.CostPrice).HasPrecision(18, 4);
            e.Property(x => x.Tax).HasPrecision(18, 4);
        });

        builder.Entity<ProductOption>(e =>
        {
            e.ToTable("ProductOptions");
            e.HasOne<Product>()
                .WithMany(x => x.ProductOptions)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductOptionValue>(e =>
        {
            e.ToTable("ProductOptionValues");
            e.HasOne<ProductOption>()
                .WithMany(x => x.ProductOptionValues)
                .HasForeignKey(x => x.ProductOptionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SkuOptionValue>(e =>
        {
            e.ToTable("SkuOptionValues");
            e.HasOne(x => x.ProductOption)
                .WithMany(x => x.SkuOptionValues)
                .HasForeignKey(x => x.ProductOptionsId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ProductOptionValue)
                .WithMany(x => x.SkuOptionValues)
                .HasForeignKey(x => x.ProductOptionValueId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ProductSku>()
                .WithMany(x => x.SkuOptionValues)
                .HasForeignKey(x => x.ProductSkuId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Orders
        builder.Entity<Order>(e =>
        {
            e.ToTable("Orders");
            e.HasOne(x => x.Partner)
                .WithMany()
                .HasForeignKey(x => x.PartnerId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.DeliveryAddress)
                .WithMany()
                .HasForeignKey(x => x.DeliveryAddressId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Items)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Status).HasConversion<int>();
        });

        builder.Entity<OrderItem>(e =>
        {
            e.ToTable("OrderItems");
            e.HasOne(x => x.Sku)
                .WithMany()
                .HasForeignKey(x => x.SkuId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.Price).HasPrecision(18, 4);
            e.Property(x => x.Tax).HasPrecision(18, 4);
            e.Property(x => x.Discount).HasPrecision(18, 4);
            e.Property(x => x.Quantity).HasPrecision(18, 4);
        });

        // Partners
        builder.Entity<Partner>(e =>
        {
            e.ToTable("Partners");
            e.HasOne(x => x.Address)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.DeliveryAddresses)
                .WithMany();
        });

        builder.Entity<Address>(e =>
        {
            e.ToTable("Addresses");
            e.HasOne(x => x.AddressType)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<AddressType>().ToTable("AddressTypes");

        // Pricing
        builder.Entity<Pricelist>(e =>
        {
            e.ToTable("Pricelists");
            e.HasMany(x => x.Items)
                .WithOne(x => x.Pricelist)
                .HasForeignKey(x => x.PricelistId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.ClientPricelists)
                .WithOne(x => x.Pricelist)
                .HasForeignKey(x => x.PricelistId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PricelistItem>(e =>
        {
            e.ToTable("PricelistItems");
            e.HasOne(x => x.ProductSku)
                .WithMany()
                .HasForeignKey(x => x.ProductSkuId)
                .OnDelete(DeleteBehavior.SetNull);
            e.Property(x => x.Price).HasPrecision(18, 4);
            e.Property(x => x.MinQuantity).HasPrecision(18, 4);
        });

        builder.Entity<ClientPricelist>(e =>
        {
            e.ToTable("ClientPricelists");
            e.HasOne(x => x.Partner)
                .WithMany()
                .HasForeignKey(x => x.PartnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ClientDiscount>(e =>
        {
            e.ToTable("ClientDiscounts");
            e.HasOne(x => x.Partner)
                .WithMany()
                .HasForeignKey(x => x.PartnerId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.DiscountPercent).HasPrecision(5, 2);
        });

        // Shopping Cart
        builder.Entity<ShoppingCartItem>(e =>
        {
            e.ToTable("ShoppingCartItems");
            e.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Price).HasPrecision(18, 4);
            e.Property(x => x.Tax).HasPrecision(18, 4);
            e.Property(x => x.Discount).HasPrecision(18, 4);
            e.Property(x => x.Quantity).HasPrecision(18, 4);
        });

        // Blog
        builder.Entity<Post>(e =>
        {
            e.ToTable("Posts");
            e.HasOne(x => x.Parent)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.User)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Categories)
                .WithMany(x => x.Posts);
            e.HasMany(x => x.Tags)
                .WithMany(x => x.Posts);
            e.HasMany(x => x.Metas)
                .WithOne(x => x.Post)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Comments)
                .WithOne(x => x.Post)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PostCategory>(e =>
        {
            e.ToTable("PostCategories");
            e.HasOne(x => x.ParentCategory)
                .WithMany(x => x.Categories)
                .HasForeignKey(x => x.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PostTag>().ToTable("PostTags");
        builder.Entity<PostMeta>().ToTable("PostMetas");
        builder.Entity<PostComment>().ToTable("PostComments");
        builder.Entity<UoM>().ToTable("UoMs");
    }
}

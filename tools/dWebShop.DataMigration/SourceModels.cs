namespace dWebShop.DataMigration;

// Raw rows read from the old MySQL database.
// Column names here match the OLD database schema the client provides.
// Adjust property names / column aliases in MigrationRunner.cs if the old schema differs.

public class SrcBrand
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LogoImage { get; set; } = string.Empty;
    public string SliderImage { get; set; } = string.Empty;
}

public class SrcCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? ParentCategoryId { get; set; }
    public int? BrandId { get; set; }
}

public class SrcProduct
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string ExtRef { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int? BrandId { get; set; }
}

// Join table product <-> category
public class SrcProductCategory
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
}

public class SrcProductDetails
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string DetailDescription { get; set; } = string.Empty;
}

public class SrcProductInfo
{
    public int Id { get; set; }
    public int ProductDetailsId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
}

public class SrcProductImage
{
    public int Id { get; set; }
    public int ProductDetailsId { get; set; }
    public string Path { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class SrcProductDocument
{
    public int Id { get; set; }
    public int ProductDetailsId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class SrcProductSku
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string ExtRef { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Tax { get; set; }
    public string? ImagePath { get; set; }
}

public class SrcProductOption
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsNamePart { get; set; }
}

public class SrcProductOptionValue
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int ProductOptionId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class SrcSkuOptionValue
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int ProductSkuId { get; set; }
    public int ProductOptionsId { get; set; }
    public int ProductOptionValueId { get; set; }
}

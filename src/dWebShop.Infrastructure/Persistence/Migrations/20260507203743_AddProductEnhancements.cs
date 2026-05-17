using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dWebShop.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductTag");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Products",
                newName: "IsFeatured");

            migrationBuilder.AddColumn<decimal>(
                name: "CompareAtPrice",
                table: "ProductSkus",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CostPrice",
                table: "ProductSkus",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gtin",
                table: "ProductSkus",
                type: "varchar(14)",
                maxLength: 14,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "LowStockThreshold",
                table: "ProductSkus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "ProductSkus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ProductTags",
                columns: table => new
                {
                    ProductsId = table.Column<int>(type: "int", nullable: false),
                    TagsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTags", x => new { x.ProductsId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_ProductTags_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductTags_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTags_TagsId",
                table: "ProductTags",
                column: "TagsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductTags");

            migrationBuilder.DropColumn(
                name: "CompareAtPrice",
                table: "ProductSkus");

            migrationBuilder.DropColumn(
                name: "CostPrice",
                table: "ProductSkus");

            migrationBuilder.DropColumn(
                name: "Gtin",
                table: "ProductSkus");

            migrationBuilder.DropColumn(
                name: "LowStockThreshold",
                table: "ProductSkus");

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "ProductSkus");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "IsFeatured",
                table: "Products",
                newName: "IsActive");

            migrationBuilder.CreateTable(
                name: "ProductTag",
                columns: table => new
                {
                    ProductsId = table.Column<int>(type: "int", nullable: false),
                    TagsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTag", x => new { x.ProductsId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_ProductTag_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductTag_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTag_TagsId",
                table: "ProductTag",
                column: "TagsId");
        }
    }
}

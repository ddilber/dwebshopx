using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dWebShop.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSkuUom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Uom",
                table: "ProductSkus",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Uom",
                table: "ProductSkus");
        }
    }
}

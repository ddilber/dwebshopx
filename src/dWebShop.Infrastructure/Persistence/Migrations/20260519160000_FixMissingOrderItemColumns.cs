using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using dWebShop.Infrastructure.Persistence;

#nullable disable

namespace dWebShop.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260519160000_FixMissingOrderItemColumns")]
    public partial class FixMissingOrderItemColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppliedRulesJson",
                table: "OrderItems",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "OrderItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalPrice",
                table: "OrderItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VatRateSnapshot",
                table: "OrderItems",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AppliedRulesJson", table: "OrderItems");
            migrationBuilder.DropColumn(name: "BasePrice", table: "OrderItems");
            migrationBuilder.DropColumn(name: "FinalPrice", table: "OrderItems");
            migrationBuilder.DropColumn(name: "VatRateSnapshot", table: "OrderItems");
        }
    }
}

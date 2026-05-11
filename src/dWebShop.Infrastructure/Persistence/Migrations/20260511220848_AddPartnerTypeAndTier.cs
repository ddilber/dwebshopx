using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dWebShop.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerTypeAndTier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PartnerType",
                table: "Partners",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Tier",
                table: "Partners",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PartnerType",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "Tier",
                table: "Partners");
        }
    }
}

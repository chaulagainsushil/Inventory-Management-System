using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Repository.Migrations
{
    /// <inheritdoc />
    public partial class changesforproductchange3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PricePerUnitPurchased",
                table: "Product",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PricePerUnitPurchased",
                table: "Product");
        }
    }
}

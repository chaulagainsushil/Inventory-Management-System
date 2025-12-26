using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Repository.Migrations
{
    /// <inheritdoc />
    public partial class changesforproductchange1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QuantityPerUnit",
                table: "Product",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuantityPerUnit",
                table: "Product");
        }
    }
}

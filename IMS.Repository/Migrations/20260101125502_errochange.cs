using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IMS.Repository.Migrations
{
    /// <inheritdoc />
    public partial class errochange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_SuppliersInfromation_SupplierId",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_SupplierId",
                table: "Product");

            migrationBuilder.AddColumn<decimal>(
                name: "AverageDailySales",
                table: "Product",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "LeadTimeDays",
                table: "Product",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReorderLevel",
                table: "Product",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SafetyStock",
                table: "Product",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SuppliersInfromationId",
                table: "Product",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "InventoryBatch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDepleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryBatch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryBatch_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Product_SuppliersInfromationId",
                table: "Product",
                column: "SuppliersInfromationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBatch_ProductId",
                table: "InventoryBatch",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_SuppliersInfromation_SuppliersInfromationId",
                table: "Product",
                column: "SuppliersInfromationId",
                principalTable: "SuppliersInfromation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_SuppliersInfromation_SuppliersInfromationId",
                table: "Product");

            migrationBuilder.DropTable(
                name: "InventoryBatch");

            migrationBuilder.DropIndex(
                name: "IX_Product_SuppliersInfromationId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "AverageDailySales",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "LeadTimeDays",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ReorderLevel",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "SafetyStock",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "SuppliersInfromationId",
                table: "Product");

            migrationBuilder.CreateIndex(
                name: "IX_Product_SupplierId",
                table: "Product",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_SuppliersInfromation_SupplierId",
                table: "Product",
                column: "SupplierId",
                principalTable: "SuppliersInfromation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

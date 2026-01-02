using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AfterAllChnages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_SuppliersInfromation_SuppliersInfromationId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "Product");

            migrationBuilder.AlterColumn<int>(
                name: "SuppliersInfromationId",
                table: "Product",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_SuppliersInfromation_SuppliersInfromationId",
                table: "Product",
                column: "SuppliersInfromationId",
                principalTable: "SuppliersInfromation",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_SuppliersInfromation_SuppliersInfromationId",
                table: "Product");

            migrationBuilder.AlterColumn<int>(
                name: "SuppliersInfromationId",
                table: "Product",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "Product",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_SuppliersInfromation_SuppliersInfromationId",
                table: "Product",
                column: "SuppliersInfromationId",
                principalTable: "SuppliersInfromation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

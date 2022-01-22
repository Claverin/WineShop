using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WineShop.Migrations
{
    public partial class ChangeValueOfProductTypeAndManufacturer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ID_ProductType",
                table: "ProductType",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ID_Manufacturer",
                table: "Manufacturer",
                newName: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ProductType",
                newName: "ID_ProductType");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Manufacturer",
                newName: "ID_Manufacturer");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WineShop.Migrations
{
    public partial class SeedInitialData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Manufacturer",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "Manufacturer",
                columns: new[] { "Id", "Country", "Name" },
                values: new object[,]
                {
                    { 1, "France", "Red Vineyards" },
                    { 2, "USA", "Golden Valley" }
                });

            migrationBuilder.InsertData(
                table: "ProductType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Red Wine" },
                    { 2, "White Wine" },
                    { 3, "Sparkling Wine" }
                });

            migrationBuilder.InsertData(
                table: "Product",
                columns: new[] { "Id", "Description", "IdManufacturer", "IdProductType", "Image", "Name", "Price", "YearOfProduction" },
                values: new object[] { 1, "A full-bodied red wine with rich notes of blackcurrant, subtle spices, and a smooth finish. Perfect for pairing with hearty meals.", 1, 1, "cabernet.png", "Cabernet Sauvignon", 29.99m, null });

            migrationBuilder.InsertData(
                table: "Product",
                columns: new[] { "Id", "Description", "IdManufacturer", "IdProductType", "Image", "Name", "Price", "YearOfProduction" },
                values: new object[] { 2, "A crisp and refreshing white wine with delicate buttery undertones and a hint of citrus. Ideal for warm evenings or light dishes.", 2, 2, "chardonnay.png", "Chardonnay", 19.99m, null });

            migrationBuilder.InsertData(
                table: "Product",
                columns: new[] { "Id", "Description", "IdManufacturer", "IdProductType", "Image", "Name", "Price", "YearOfProduction" },
                values: new object[] { 3, "A sparkling wine with vibrant bubbles and fresh flavors of green apple and pear. Celebrate life's moments with its joyful character.", 2, 3, "prosecco.png", "Prosecco", 15.99m, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Manufacturer",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Manufacturer",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ProductType",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ProductType",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ProductType",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Manufacturer");
        }
    }
}

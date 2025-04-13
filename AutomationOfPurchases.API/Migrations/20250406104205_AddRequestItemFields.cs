using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomationOfPurchases.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestItemFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliveredQuantity",
                table: "RequestItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ToPurchaseQuantity",
                table: "RequestItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveredQuantity",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "ToPurchaseQuantity",
                table: "RequestItems");
        }
    }
}

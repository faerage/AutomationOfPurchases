using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomationOfPurchases.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Notifications",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Notifications");
        }
    }
}

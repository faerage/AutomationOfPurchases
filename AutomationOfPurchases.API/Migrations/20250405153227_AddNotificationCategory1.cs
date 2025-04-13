using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomationOfPurchases.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationCategory1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequestId",
                table: "Notifications",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "Notifications");
        }
    }
}

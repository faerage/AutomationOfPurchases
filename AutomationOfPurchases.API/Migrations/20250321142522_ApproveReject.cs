using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomationOfPurchases.API.Migrations
{
    /// <inheritdoc />
    public partial class ApproveReject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DepartmentHeadApproverId",
                table: "Requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EconomistApproverId",
                table: "Requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectedByUserId",
                table: "Requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Requests",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_DepartmentHeadApproverId",
                table: "Requests",
                column: "DepartmentHeadApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_EconomistApproverId",
                table: "Requests",
                column: "EconomistApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_RejectedByUserId",
                table: "Requests",
                column: "RejectedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_AspNetUsers_DepartmentHeadApproverId",
                table: "Requests",
                column: "DepartmentHeadApproverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_AspNetUsers_EconomistApproverId",
                table: "Requests",
                column: "EconomistApproverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_AspNetUsers_RejectedByUserId",
                table: "Requests",
                column: "RejectedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_AspNetUsers_DepartmentHeadApproverId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_AspNetUsers_EconomistApproverId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_AspNetUsers_RejectedByUserId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_DepartmentHeadApproverId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_EconomistApproverId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_RejectedByUserId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "DepartmentHeadApproverId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "EconomistApproverId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "RejectedByUserId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Requests");
        }
    }
}

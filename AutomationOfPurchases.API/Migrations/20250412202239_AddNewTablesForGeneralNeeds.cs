using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutomationOfPurchases.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTablesForGeneralNeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestItems_GeneralNeedsLists_GeneralNeedsListListId",
                table: "RequestItems");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestItems_NetNeedsLists_NetNeedsListListId",
                table: "RequestItems");

            migrationBuilder.DropIndex(
                name: "IX_RequestItems_GeneralNeedsListListId",
                table: "RequestItems");

            migrationBuilder.DropIndex(
                name: "IX_RequestItems_NetNeedsListListId",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "GeneralNeedsListListId",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "NetNeedsListListId",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "OriginalRequestId",
                table: "RequestItems");

            migrationBuilder.CreateTable(
                name: "GeneralNeedsItems",
                columns: table => new
                {
                    GeneralNeedsItemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GeneralNeedsListId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    OrderedById = table.Column<string>(type: "text", nullable: true),
                    OriginalRequestItemId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralNeedsItems", x => x.GeneralNeedsItemId);
                    table.ForeignKey(
                        name: "FK_GeneralNeedsItems_AspNetUsers_OrderedById",
                        column: x => x.OrderedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GeneralNeedsItems_GeneralNeedsLists_GeneralNeedsListId",
                        column: x => x.GeneralNeedsListId,
                        principalTable: "GeneralNeedsLists",
                        principalColumn: "ListId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeneralNeedsItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeneralNeedsItems_RequestItems_OriginalRequestItemId",
                        column: x => x.OriginalRequestItemId,
                        principalTable: "RequestItems",
                        principalColumn: "RequestItemId");
                });

            migrationBuilder.CreateTable(
                name: "NetNeedsItems",
                columns: table => new
                {
                    NetNeedsItemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NetNeedsListId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    OrderedById = table.Column<string>(type: "text", nullable: true),
                    OriginalRequestItemId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetNeedsItems", x => x.NetNeedsItemId);
                    table.ForeignKey(
                        name: "FK_NetNeedsItems_AspNetUsers_OrderedById",
                        column: x => x.OrderedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NetNeedsItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NetNeedsItems_NetNeedsLists_NetNeedsListId",
                        column: x => x.NetNeedsListId,
                        principalTable: "NetNeedsLists",
                        principalColumn: "ListId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NetNeedsItems_RequestItems_OriginalRequestItemId",
                        column: x => x.OriginalRequestItemId,
                        principalTable: "RequestItems",
                        principalColumn: "RequestItemId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneralNeedsItems_GeneralNeedsListId",
                table: "GeneralNeedsItems",
                column: "GeneralNeedsListId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralNeedsItems_ItemId",
                table: "GeneralNeedsItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralNeedsItems_OrderedById",
                table: "GeneralNeedsItems",
                column: "OrderedById");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralNeedsItems_OriginalRequestItemId",
                table: "GeneralNeedsItems",
                column: "OriginalRequestItemId");

            migrationBuilder.CreateIndex(
                name: "IX_NetNeedsItems_ItemId",
                table: "NetNeedsItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_NetNeedsItems_NetNeedsListId",
                table: "NetNeedsItems",
                column: "NetNeedsListId");

            migrationBuilder.CreateIndex(
                name: "IX_NetNeedsItems_OrderedById",
                table: "NetNeedsItems",
                column: "OrderedById");

            migrationBuilder.CreateIndex(
                name: "IX_NetNeedsItems_OriginalRequestItemId",
                table: "NetNeedsItems",
                column: "OriginalRequestItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneralNeedsItems");

            migrationBuilder.DropTable(
                name: "NetNeedsItems");

            migrationBuilder.AddColumn<int>(
                name: "GeneralNeedsListListId",
                table: "RequestItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NetNeedsListListId",
                table: "RequestItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginalRequestId",
                table: "RequestItems",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestItems_GeneralNeedsListListId",
                table: "RequestItems",
                column: "GeneralNeedsListListId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestItems_NetNeedsListListId",
                table: "RequestItems",
                column: "NetNeedsListListId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestItems_GeneralNeedsLists_GeneralNeedsListListId",
                table: "RequestItems",
                column: "GeneralNeedsListListId",
                principalTable: "GeneralNeedsLists",
                principalColumn: "ListId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestItems_NetNeedsLists_NetNeedsListListId",
                table: "RequestItems",
                column: "NetNeedsListListId",
                principalTable: "NetNeedsLists",
                principalColumn: "ListId");
        }
    }
}

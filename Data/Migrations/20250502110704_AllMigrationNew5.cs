using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialWelfarre.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllMigrationNew5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationFoodPack_StockIn_FoodPacks_StockInId",
                table: "ApplicationFoodPack");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "DisasterKitTransactions",
                type: "nvarchar(34)",
                maxLength: 34,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestDate1",
                table: "DisasterKitTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StockInId1",
                table: "DisasterKitTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalPacks1",
                table: "DisasterKitTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StockIn_DisasterKit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Add_Stock1 = table.Column<int>(type: "int", nullable: false),
                    StockInDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockIn_DisasterKit", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DisasterKitTransactions_StockInId1",
                table: "DisasterKitTransactions",
                column: "StockInId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationFoodPack_StockIn_FoodPacks_StockInId",
                table: "ApplicationFoodPack",
                column: "StockInId",
                principalTable: "StockIn_FoodPacks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DisasterKitTransactions_StockIn_DisasterKit_StockInId1",
                table: "DisasterKitTransactions",
                column: "StockInId1",
                principalTable: "StockIn_DisasterKit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationFoodPack_StockIn_FoodPacks_StockInId",
                table: "ApplicationFoodPack");

            migrationBuilder.DropForeignKey(
                name: "FK_DisasterKitTransactions_StockIn_DisasterKit_StockInId1",
                table: "DisasterKitTransactions");

            migrationBuilder.DropTable(
                name: "StockIn_DisasterKit");

            migrationBuilder.DropIndex(
                name: "IX_DisasterKitTransactions_StockInId1",
                table: "DisasterKitTransactions");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "DisasterKitTransactions");

            migrationBuilder.DropColumn(
                name: "RequestDate1",
                table: "DisasterKitTransactions");

            migrationBuilder.DropColumn(
                name: "StockInId1",
                table: "DisasterKitTransactions");

            migrationBuilder.DropColumn(
                name: "TotalPacks1",
                table: "DisasterKitTransactions");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationFoodPack_StockIn_FoodPacks_StockInId",
                table: "ApplicationFoodPack",
                column: "StockInId",
                principalTable: "StockIn_FoodPacks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

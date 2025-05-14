using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialWelfarre.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllMigrationNew4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "ApplicationFoodPack",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestDate",
                table: "ApplicationFoodPack",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StockInId",
                table: "ApplicationFoodPack",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalPacks",
                table: "ApplicationFoodPack",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StockIn_FoodPacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Add_Stock = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockIn_FoodPacks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationFoodPack_StockInId",
                table: "ApplicationFoodPack",
                column: "StockInId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationFoodPack_StockIn_FoodPacks_StockInId",
                table: "ApplicationFoodPack",
                column: "StockInId",
                principalTable: "StockIn_FoodPacks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationFoodPack_StockIn_FoodPacks_StockInId",
                table: "ApplicationFoodPack");

            migrationBuilder.DropTable(
                name: "StockIn_FoodPacks");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationFoodPack_StockInId",
                table: "ApplicationFoodPack");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "ApplicationFoodPack");

            migrationBuilder.DropColumn(
                name: "RequestDate",
                table: "ApplicationFoodPack");

            migrationBuilder.DropColumn(
                name: "StockInId",
                table: "ApplicationFoodPack");

            migrationBuilder.DropColumn(
                name: "TotalPacks",
                table: "ApplicationFoodPack");
        }
    }
}

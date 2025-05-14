using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialWelfarre.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllNewMigration6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationFoodPack_StockIn_FoodPacks_StockInId",
                table: "ApplicationFoodPack");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationFoodPack_StockInId",
                table: "ApplicationFoodPack");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "ApplicationFoodPack");

            migrationBuilder.DropColumn(
                name: "StockInId",
                table: "ApplicationFoodPack");

            migrationBuilder.DropColumn(
                name: "TotalPacks",
                table: "ApplicationFoodPack");

            migrationBuilder.RenameColumn(
                name: "Add_Stock",
                table: "StockIn_FoodPacks",
                newName: "Add_Stock2");

            migrationBuilder.AddColumn<DateTime>(
                name: "Restock_DateTime2",
                table: "StockIn_FoodPacks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<DateTime>(
                name: "RequestDate",
                table: "ApplicationFoodPack",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "FoodPackInventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationFoodPackId = table.Column<int>(type: "int", nullable: false),
                    StockInId = table.Column<int>(type: "int", nullable: false),
                    TotalPacks = table.Column<int>(type: "int", nullable: false),
                    StockLeft = table.Column<int>(type: "int", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Barangay = table.Column<int>(type: "int", nullable: false),
                    Packs = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodPackInventories", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoodPackInventories");

            migrationBuilder.DropColumn(
                name: "Restock_DateTime2",
                table: "StockIn_FoodPacks");

            migrationBuilder.RenameColumn(
                name: "Add_Stock2",
                table: "StockIn_FoodPacks",
                newName: "Add_Stock");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RequestDate",
                table: "ApplicationFoodPack",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "ApplicationFoodPack",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

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
                onDelete: ReferentialAction.Restrict);
        }
    }
}

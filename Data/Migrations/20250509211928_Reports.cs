using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialWelfarre.Data.Migrations
{
    /// <inheritdoc />
    public partial class Reports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DisasterKitTransactions_StockIn_DisasterKit_StockInId1",
                table: "DisasterKitTransactions");

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

            migrationBuilder.CreateTable(
                name: "DisasterKitInventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockInId1 = table.Column<int>(type: "int", nullable: false),
                    TotalPacks1 = table.Column<int>(type: "int", nullable: false),
                    StockLeft = table.Column<int>(type: "int", nullable: false),
                    RequestDate1 = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Barangay3 = table.Column<int>(type: "int", nullable: false),
                    Reason3 = table.Column<int>(type: "int", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    NumberOfPacks3 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisasterKitInventories", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisasterKitInventories");

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

            migrationBuilder.CreateIndex(
                name: "IX_DisasterKitTransactions_StockInId1",
                table: "DisasterKitTransactions",
                column: "StockInId1");

            migrationBuilder.AddForeignKey(
                name: "FK_DisasterKitTransactions_StockIn_DisasterKit_StockInId1",
                table: "DisasterKitTransactions",
                column: "StockInId1",
                principalTable: "StockIn_DisasterKit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

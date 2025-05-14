using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialWelfarre.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllNewMigration8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FoodPackInventories",
                table: "FoodPackInventories");

            migrationBuilder.RenameTable(
                name: "FoodPackInventories",
                newName: "FoodPackInventory");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodPackInventory",
                table: "FoodPackInventory",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FoodPackInventory",
                table: "FoodPackInventory");

            migrationBuilder.RenameTable(
                name: "FoodPackInventory",
                newName: "FoodPackInventories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodPackInventories",
                table: "FoodPackInventories",
                column: "Id");
        }
    }
}

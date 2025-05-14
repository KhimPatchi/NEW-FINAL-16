using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialWelfarre.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllMigrationsNew2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Age",
                table: "Certificate_Of_Indigencies",
                newName: "Age1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Age1",
                table: "Certificate_Of_Indigencies",
                newName: "Age");
        }
    }
}

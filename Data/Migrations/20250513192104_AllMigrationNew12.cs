using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialWelfarre.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllMigrationNew12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovedById",
                table: "Consultations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Reason1",
                table: "Certificate_Of_Indigencies",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Barangay1",
                table: "Certificate_Of_Indigencies",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedById",
                table: "Certificate_Of_Indigencies",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Reason",
                table: "ApplicationFoodPack",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Barangay",
                table: "ApplicationFoodPack",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedById",
                table: "ApplicationFoodPack",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_ApprovedById",
                table: "Consultations",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_Of_Indigencies_ApprovedById",
                table: "Certificate_Of_Indigencies",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationFoodPack_ApprovedById",
                table: "ApplicationFoodPack",
                column: "ApprovedById");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationFoodPack_AspNetUsers_ApprovedById",
                table: "ApplicationFoodPack",
                column: "ApprovedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificate_Of_Indigencies_AspNetUsers_ApprovedById",
                table: "Certificate_Of_Indigencies",
                column: "ApprovedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_AspNetUsers_ApprovedById",
                table: "Consultations",
                column: "ApprovedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationFoodPack_AspNetUsers_ApprovedById",
                table: "ApplicationFoodPack");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificate_Of_Indigencies_AspNetUsers_ApprovedById",
                table: "Certificate_Of_Indigencies");

            migrationBuilder.DropForeignKey(
                name: "FK_Consultations_AspNetUsers_ApprovedById",
                table: "Consultations");

            migrationBuilder.DropIndex(
                name: "IX_Consultations_ApprovedById",
                table: "Consultations");

            migrationBuilder.DropIndex(
                name: "IX_Certificate_Of_Indigencies_ApprovedById",
                table: "Certificate_Of_Indigencies");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationFoodPack_ApprovedById",
                table: "ApplicationFoodPack");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "Certificate_Of_Indigencies");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "ApplicationFoodPack");

            migrationBuilder.AlterColumn<int>(
                name: "Reason1",
                table: "Certificate_Of_Indigencies",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Barangay1",
                table: "Certificate_Of_Indigencies",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Reason",
                table: "ApplicationFoodPack",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Barangay",
                table: "ApplicationFoodPack",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}

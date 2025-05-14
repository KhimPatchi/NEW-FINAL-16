using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialWelfarre.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllMigrationsNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "DisasterKitTransactions",
                newName: "Reason3");

            migrationBuilder.RenameColumn(
                name: "NumberOfPacks",
                table: "DisasterKitTransactions",
                newName: "NumberOfPacks3");

            migrationBuilder.RenameColumn(
                name: "Barangay",
                table: "DisasterKitTransactions",
                newName: "Barangay3");

            migrationBuilder.RenameColumn(
                name: "x1_Pic_Path",
                table: "Consultations",
                newName: "x1_Pic_Path2");

            migrationBuilder.RenameColumn(
                name: "x1_Pic",
                table: "Consultations",
                newName: "x1_Pic2");

            migrationBuilder.RenameColumn(
                name: "Valid_ID_Path",
                table: "Consultations",
                newName: "Valid_ID_Path2");

            migrationBuilder.RenameColumn(
                name: "Valid_ID",
                table: "Consultations",
                newName: "Valid_ID2");

            migrationBuilder.RenameColumn(
                name: "Proof_SoloParent_Path",
                table: "Consultations",
                newName: "Proof_SoloParent_Path2");

            migrationBuilder.RenameColumn(
                name: "Proof_SoloParent",
                table: "Consultations",
                newName: "Proof_SoloParent2");

            migrationBuilder.RenameColumn(
                name: "Middle_Name",
                table: "Consultations",
                newName: "Middle_Name2");

            migrationBuilder.RenameColumn(
                name: "Last_Name",
                table: "Consultations",
                newName: "Last_Name2");

            migrationBuilder.RenameColumn(
                name: "First_Name",
                table: "Consultations",
                newName: "First_Name2");

            migrationBuilder.RenameColumn(
                name: "Brgy_Cert_Path",
                table: "Consultations",
                newName: "Brgy_Cert_Path2");

            migrationBuilder.RenameColumn(
                name: "Brgy_Cert",
                table: "Consultations",
                newName: "Brgy_Cert2");

            migrationBuilder.RenameColumn(
                name: "Birth_Cert_Path",
                table: "Consultations",
                newName: "Birth_Cert_Path2");

            migrationBuilder.RenameColumn(
                name: "Birth_Cert",
                table: "Consultations",
                newName: "Birth_Cert2");

            migrationBuilder.RenameColumn(
                name: "Valid_ID_Path",
                table: "Certificate_Of_Indigencies",
                newName: "Valid_ID_Path1");

            migrationBuilder.RenameColumn(
                name: "Valid_ID",
                table: "Certificate_Of_Indigencies",
                newName: "Valid_ID1");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Certificate_Of_Indigencies",
                newName: "Status1");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "Certificate_Of_Indigencies",
                newName: "Reason1");

            migrationBuilder.RenameColumn(
                name: "No_Rquested",
                table: "Certificate_Of_Indigencies",
                newName: "No_Rquested1");

            migrationBuilder.RenameColumn(
                name: "MiddleName",
                table: "Certificate_Of_Indigencies",
                newName: "MiddleName1");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Certificate_Of_Indigencies",
                newName: "LastName1");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Certificate_Of_Indigencies",
                newName: "FirstName1");

            migrationBuilder.RenameColumn(
                name: "ContactNumber",
                table: "Certificate_Of_Indigencies",
                newName: "ContactNumber1");

            migrationBuilder.RenameColumn(
                name: "Brgy_Cert_Path",
                table: "Certificate_Of_Indigencies",
                newName: "Brgy_Cert_Path1");

            migrationBuilder.RenameColumn(
                name: "Brgy_Cert",
                table: "Certificate_Of_Indigencies",
                newName: "Brgy_Cert1");

            migrationBuilder.RenameColumn(
                name: "Barangay",
                table: "Certificate_Of_Indigencies",
                newName: "Barangay1");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Certificate_Of_Indigencies",
                newName: "Address1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Reason3",
                table: "DisasterKitTransactions",
                newName: "Reason");

            migrationBuilder.RenameColumn(
                name: "NumberOfPacks3",
                table: "DisasterKitTransactions",
                newName: "NumberOfPacks");

            migrationBuilder.RenameColumn(
                name: "Barangay3",
                table: "DisasterKitTransactions",
                newName: "Barangay");

            migrationBuilder.RenameColumn(
                name: "x1_Pic_Path2",
                table: "Consultations",
                newName: "x1_Pic_Path");

            migrationBuilder.RenameColumn(
                name: "x1_Pic2",
                table: "Consultations",
                newName: "x1_Pic");

            migrationBuilder.RenameColumn(
                name: "Valid_ID_Path2",
                table: "Consultations",
                newName: "Valid_ID_Path");

            migrationBuilder.RenameColumn(
                name: "Valid_ID2",
                table: "Consultations",
                newName: "Valid_ID");

            migrationBuilder.RenameColumn(
                name: "Proof_SoloParent_Path2",
                table: "Consultations",
                newName: "Proof_SoloParent_Path");

            migrationBuilder.RenameColumn(
                name: "Proof_SoloParent2",
                table: "Consultations",
                newName: "Proof_SoloParent");

            migrationBuilder.RenameColumn(
                name: "Middle_Name2",
                table: "Consultations",
                newName: "Middle_Name");

            migrationBuilder.RenameColumn(
                name: "Last_Name2",
                table: "Consultations",
                newName: "Last_Name");

            migrationBuilder.RenameColumn(
                name: "First_Name2",
                table: "Consultations",
                newName: "First_Name");

            migrationBuilder.RenameColumn(
                name: "Brgy_Cert_Path2",
                table: "Consultations",
                newName: "Brgy_Cert_Path");

            migrationBuilder.RenameColumn(
                name: "Brgy_Cert2",
                table: "Consultations",
                newName: "Brgy_Cert");

            migrationBuilder.RenameColumn(
                name: "Birth_Cert_Path2",
                table: "Consultations",
                newName: "Birth_Cert_Path");

            migrationBuilder.RenameColumn(
                name: "Birth_Cert2",
                table: "Consultations",
                newName: "Birth_Cert");

            migrationBuilder.RenameColumn(
                name: "Valid_ID_Path1",
                table: "Certificate_Of_Indigencies",
                newName: "Valid_ID_Path");

            migrationBuilder.RenameColumn(
                name: "Valid_ID1",
                table: "Certificate_Of_Indigencies",
                newName: "Valid_ID");

            migrationBuilder.RenameColumn(
                name: "Status1",
                table: "Certificate_Of_Indigencies",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Reason1",
                table: "Certificate_Of_Indigencies",
                newName: "Reason");

            migrationBuilder.RenameColumn(
                name: "No_Rquested1",
                table: "Certificate_Of_Indigencies",
                newName: "No_Rquested");

            migrationBuilder.RenameColumn(
                name: "MiddleName1",
                table: "Certificate_Of_Indigencies",
                newName: "MiddleName");

            migrationBuilder.RenameColumn(
                name: "LastName1",
                table: "Certificate_Of_Indigencies",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "FirstName1",
                table: "Certificate_Of_Indigencies",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "ContactNumber1",
                table: "Certificate_Of_Indigencies",
                newName: "ContactNumber");

            migrationBuilder.RenameColumn(
                name: "Brgy_Cert_Path1",
                table: "Certificate_Of_Indigencies",
                newName: "Brgy_Cert_Path");

            migrationBuilder.RenameColumn(
                name: "Brgy_Cert1",
                table: "Certificate_Of_Indigencies",
                newName: "Brgy_Cert");

            migrationBuilder.RenameColumn(
                name: "Barangay1",
                table: "Certificate_Of_Indigencies",
                newName: "Barangay");

            migrationBuilder.RenameColumn(
                name: "Address1",
                table: "Certificate_Of_Indigencies",
                newName: "Address");
        }
    }
}

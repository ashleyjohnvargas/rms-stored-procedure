using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaseDetailsTableInDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LeaseStatus",
                table: "Leases",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "Active");

            migrationBuilder.AddColumn<int>(
                name: "LeaseDuration",
                table: "Leases",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TermsAndConditions",
                table: "Leases",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LeaseDetails",
                columns: table => new
                {
                    LeaseDetailsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeaseID = table.Column<int>(type: "int", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmploymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonthlyIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaseDetails", x => x.LeaseDetailsId);
                    table.ForeignKey(
                        name: "FK_LeaseDetails_Leases_LeaseID",
                        column: x => x.LeaseID,
                        principalTable: "Leases",
                        principalColumn: "LeaseID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaseDetails_LeaseID",
                table: "LeaseDetails",
                column: "LeaseID",
                unique: true,
                filter: "[LeaseID] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaseDetails");

            migrationBuilder.DropColumn(
                name: "LeaseDuration",
                table: "Leases");

            migrationBuilder.DropColumn(
                name: "TermsAndConditions",
                table: "Leases");

            migrationBuilder.AlterColumn<string>(
                name: "LeaseStatus",
                table: "Leases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Active",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldDefaultValue: "Pending");
        }
    }
}

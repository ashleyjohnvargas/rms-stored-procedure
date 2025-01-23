using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestAndStaffTableInDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_Staffs_StaffID",
                table: "MaintenanceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_Tenants_TenantID",
                table: "MaintenanceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_Units_UnitID",
                table: "MaintenanceRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MaintenanceRequests",
                table: "MaintenanceRequests");

            migrationBuilder.RenameTable(
                name: "MaintenanceRequests",
                newName: "MaintenanceRequest");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceRequests_UnitID",
                table: "MaintenanceRequest",
                newName: "IX_MaintenanceRequest_UnitID");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceRequests_TenantID",
                table: "MaintenanceRequest",
                newName: "IX_MaintenanceRequest_TenantID");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceRequests_StaffID",
                table: "MaintenanceRequest",
                newName: "IX_MaintenanceRequest_StaffID");

            migrationBuilder.AddColumn<bool>(
                name: "IsVacant",
                table: "Staffs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShiftEndTime",
                table: "Staffs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShiftStartTime",
                table: "Staffs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MaintenanceRequest",
                table: "MaintenanceRequest",
                column: "RequestID");

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    RequestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantID = table.Column<int>(type: "int", nullable: true),
                    StaffID = table.Column<int>(type: "int", nullable: true),
                    RequestType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestStartDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.RequestID);
                    table.ForeignKey(
                        name: "FK_Requests_Staffs_StaffID",
                        column: x => x.StaffID,
                        principalTable: "Staffs",
                        principalColumn: "StaffID");
                    table.ForeignKey(
                        name: "FK_Requests_Tenants_TenantID",
                        column: x => x.TenantID,
                        principalTable: "Tenants",
                        principalColumn: "TenantID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_StaffID",
                table: "Requests",
                column: "StaffID");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_TenantID",
                table: "Requests",
                column: "TenantID");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequest_Staffs_StaffID",
                table: "MaintenanceRequest",
                column: "StaffID",
                principalTable: "Staffs",
                principalColumn: "StaffID");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequest_Tenants_TenantID",
                table: "MaintenanceRequest",
                column: "TenantID",
                principalTable: "Tenants",
                principalColumn: "TenantID");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequest_Units_UnitID",
                table: "MaintenanceRequest",
                column: "UnitID",
                principalTable: "Units",
                principalColumn: "UnitID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequest_Staffs_StaffID",
                table: "MaintenanceRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequest_Tenants_TenantID",
                table: "MaintenanceRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequest_Units_UnitID",
                table: "MaintenanceRequest");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MaintenanceRequest",
                table: "MaintenanceRequest");

            migrationBuilder.DropColumn(
                name: "IsVacant",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "ShiftEndTime",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "ShiftStartTime",
                table: "Staffs");

            migrationBuilder.RenameTable(
                name: "MaintenanceRequest",
                newName: "MaintenanceRequests");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceRequest_UnitID",
                table: "MaintenanceRequests",
                newName: "IX_MaintenanceRequests_UnitID");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceRequest_TenantID",
                table: "MaintenanceRequests",
                newName: "IX_MaintenanceRequests_TenantID");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceRequest_StaffID",
                table: "MaintenanceRequests",
                newName: "IX_MaintenanceRequests_StaffID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MaintenanceRequests",
                table: "MaintenanceRequests",
                column: "RequestID");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_Staffs_StaffID",
                table: "MaintenanceRequests",
                column: "StaffID",
                principalTable: "Staffs",
                principalColumn: "StaffID");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_Tenants_TenantID",
                table: "MaintenanceRequests",
                column: "TenantID",
                principalTable: "Tenants",
                principalColumn: "TenantID");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_Units_UnitID",
                table: "MaintenanceRequests",
                column: "UnitID",
                principalTable: "Units",
                principalColumn: "UnitID");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActualTenantFieldInTenantsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "PropertyManagers");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActualTenant",
                table: "Tenants",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsActualTenant",
                table: "Tenants");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Staffs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "PropertyManagers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Migrations
{
    /// <inheritdoc />
    public partial class SetAvailabilityStatusToNotNullInUnitsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AvailabilityStatus",
                table: "Units",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Available",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldDefaultValue: "Available");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AvailabilityStatus",
                table: "Units",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "Available",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "Available");
        }
    }
}

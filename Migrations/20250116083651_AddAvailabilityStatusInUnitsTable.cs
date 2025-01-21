using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Migrations
{
    /// <inheritdoc />
    public partial class AddAvailabilityStatusInUnitsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvailabilityStatus",
                table: "Units",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "Available");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailabilityStatus",
                table: "Units");
        }
    }
}

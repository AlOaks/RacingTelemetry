using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RacingTelemetry.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverIdToLap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DriverId",
                table: "Laps",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "Laps");
        }
    }
}

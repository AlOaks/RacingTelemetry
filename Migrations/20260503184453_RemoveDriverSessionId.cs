using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RacingTelemetry.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDriverSessionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "DriverSessions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "DriverSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}

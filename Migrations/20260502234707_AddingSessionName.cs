using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RacingTelemetry.Migrations
{
    /// <inheritdoc />
    public partial class AddingSessionName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SessionName",
                table: "Sessions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionName",
                table: "Sessions");
        }
    }
}

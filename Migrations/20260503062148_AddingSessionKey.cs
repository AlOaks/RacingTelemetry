using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RacingTelemetry.Migrations
{
    /// <inheritdoc />
    public partial class AddingSessionKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SessionKey",
                table: "Sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionKey",
                table: "Sessions");
        }
    }
}

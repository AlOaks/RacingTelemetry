using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RacingTelemetry.Migrations
{
    /// <inheritdoc />
    public partial class AddingDriverSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Drivers_DriverId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_DriverId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "Sessions");

            migrationBuilder.CreateTable(
                name: "DriverSessions",
                columns: table => new
                {
                    DriverId = table.Column<int>(type: "integer", nullable: false),
                    SessionId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverSessions", x => new { x.DriverId, x.SessionId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverSessions");

            migrationBuilder.AddColumn<int>(
                name: "DriverId",
                table: "Sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_DriverId",
                table: "Sessions",
                column: "DriverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Drivers_DriverId",
                table: "Sessions",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

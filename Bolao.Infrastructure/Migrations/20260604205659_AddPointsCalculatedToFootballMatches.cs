using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bolao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPointsCalculatedToFootballMatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PointsCalculated",
                table: "FootballMatches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatches_Status_PointsCalculated",
                table: "FootballMatches",
                columns: new[] { "Status", "PointsCalculated" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FootballMatches_Status_PointsCalculated",
                table: "FootballMatches");

            migrationBuilder.DropColumn(
                name: "PointsCalculated",
                table: "FootballMatches");
        }
    }
}

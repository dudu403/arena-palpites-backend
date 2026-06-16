using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bolao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPredictionStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AwayGoalsHit",
                table: "Predictions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ExactScoreHit",
                table: "Predictions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HomeGoalsHit",
                table: "Predictions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WinnerHit",
                table: "Predictions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwayGoalsHit",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "ExactScoreHit",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "HomeGoalsHit",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "WinnerHit",
                table: "Predictions");
        }
    }
}

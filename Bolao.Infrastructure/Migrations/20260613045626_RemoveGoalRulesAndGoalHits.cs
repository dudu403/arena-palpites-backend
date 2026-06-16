using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bolao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGoalRulesAndGoalHits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwayGoalsHit",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "HomeGoalsHit",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "GoalsPoints",
                table: "BolaoRules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AwayGoalsHit",
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

            migrationBuilder.AddColumn<int>(
                name: "GoalsPoints",
                table: "BolaoRules",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}

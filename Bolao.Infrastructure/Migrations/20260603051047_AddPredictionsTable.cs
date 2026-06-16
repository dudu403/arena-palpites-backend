using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bolao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPredictionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Performance",
                table: "FootballGroupStandings",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateTable(
                name: "Predictions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BolaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FootballMatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HomeScore = table.Column<int>(type: "int", nullable: false),
                    AwayScore = table.Column<int>(type: "int", nullable: false),
                    PointsEarned = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Predictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Predictions_Boloes_BolaoId",
                        column: x => x.BolaoId,
                        principalTable: "Boloes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Predictions_FootballMatches_FootballMatchId",
                        column: x => x.FootballMatchId,
                        principalTable: "FootballMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Predictions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_BolaoId",
                table: "Predictions",
                column: "BolaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_BolaoId_UserId_FootballMatchId",
                table: "Predictions",
                columns: new[] { "BolaoId", "UserId", "FootballMatchId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_FootballMatchId",
                table: "Predictions",
                column: "FootballMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_UserId",
                table: "Predictions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Predictions");

            migrationBuilder.AlterColumn<decimal>(
                name: "Performance",
                table: "FootballGroupStandings",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);
        }
    }
}

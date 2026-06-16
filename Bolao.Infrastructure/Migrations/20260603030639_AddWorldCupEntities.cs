using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bolao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorldCupEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Championships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    PopularName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Season = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Region = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Championships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FootballGroupStandings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChampionshipExternalId = table.Column<int>(type: "int", nullable: false),
                    PhaseExternalId = table.Column<int>(type: "int", nullable: true),
                    GroupName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    GroupSlug = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    TeamExternalId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Games = table.Column<int>(type: "int", nullable: false),
                    Wins = table.Column<int>(type: "int", nullable: false),
                    Draws = table.Column<int>(type: "int", nullable: false),
                    Losses = table.Column<int>(type: "int", nullable: false),
                    GoalsFor = table.Column<int>(type: "int", nullable: false),
                    GoalsAgainst = table.Column<int>(type: "int", nullable: false),
                    GoalDifference = table.Column<int>(type: "int", nullable: false),
                    Performance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PositionVariation = table.Column<int>(type: "int", nullable: false),
                    QualificationZone = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballGroupStandings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FootballMatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalId = table.Column<int>(type: "int", nullable: false),
                    ChampionshipExternalId = table.Column<int>(type: "int", nullable: false),
                    PhaseExternalId = table.Column<int>(type: "int", nullable: true),
                    GroupName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    GroupSlug = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    RoundName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    RoundSlug = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: true),
                    ScoreText = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    HomeTeamExternalId = table.Column<int>(type: "int", nullable: false),
                    AwayTeamExternalId = table.Column<int>(type: "int", nullable: false),
                    HomeScore = table.Column<int>(type: "int", nullable: true),
                    AwayScore = table.Column<int>(type: "int", nullable: true),
                    HasPenaltyShootout = table.Column<bool>(type: "bit", nullable: false),
                    MatchDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MatchDateText = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MatchTimeText = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    StadiumExternalId = table.Column<int>(type: "int", nullable: true),
                    StadiumName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballMatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FootballTeams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Acronym = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballTeams", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Championships_ExternalId",
                table: "Championships",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FootballGroupStandings_ChampionshipExternalId_PhaseExternalId_GroupSlug_TeamExternalId",
                table: "FootballGroupStandings",
                columns: new[] { "ChampionshipExternalId", "PhaseExternalId", "GroupSlug", "TeamExternalId" },
                unique: true,
                filter: "[PhaseExternalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FootballGroupStandings_TeamExternalId",
                table: "FootballGroupStandings",
                column: "TeamExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatches_AwayTeamExternalId",
                table: "FootballMatches",
                column: "AwayTeamExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatches_ChampionshipExternalId",
                table: "FootballMatches",
                column: "ChampionshipExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatches_ChampionshipExternalId_RoundNumber",
                table: "FootballMatches",
                columns: new[] { "ChampionshipExternalId", "RoundNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatches_ExternalId",
                table: "FootballMatches",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatches_HomeTeamExternalId",
                table: "FootballMatches",
                column: "HomeTeamExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatches_MatchDate",
                table: "FootballMatches",
                column: "MatchDate");

            migrationBuilder.CreateIndex(
                name: "IX_FootballTeams_ExternalId",
                table: "FootballTeams",
                column: "ExternalId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Championships");

            migrationBuilder.DropTable(
                name: "FootballGroupStandings");

            migrationBuilder.DropTable(
                name: "FootballMatches");

            migrationBuilder.DropTable(
                name: "FootballTeams");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bolao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateBolaoTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Boloes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Championship = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MaxParticipants = table.Column<int>(type: "int", nullable: false),
                    Privacy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InviteCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boloes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Boloes_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BolaoMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BolaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BolaoMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BolaoMembers_Boloes_BolaoId",
                        column: x => x.BolaoId,
                        principalTable: "Boloes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BolaoMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BolaoRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BolaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExactScorePoints = table.Column<int>(type: "int", nullable: false),
                    WinnerPoints = table.Column<int>(type: "int", nullable: false),
                    GoalsPoints = table.Column<int>(type: "int", nullable: false),
                    DrawPoints = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BolaoRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BolaoRules_Boloes_BolaoId",
                        column: x => x.BolaoId,
                        principalTable: "Boloes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BolaoMembers_BolaoId_UserId",
                table: "BolaoMembers",
                columns: new[] { "BolaoId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BolaoMembers_UserId",
                table: "BolaoMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BolaoRules_BolaoId",
                table: "BolaoRules",
                column: "BolaoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Boloes_InviteCode",
                table: "Boloes",
                column: "InviteCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Boloes_OwnerId",
                table: "Boloes",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BolaoMembers");

            migrationBuilder.DropTable(
                name: "BolaoRules");

            migrationBuilder.DropTable(
                name: "Boloes");
        }
    }
}

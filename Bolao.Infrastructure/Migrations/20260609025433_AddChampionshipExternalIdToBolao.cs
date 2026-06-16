using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bolao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChampionshipExternalIdToBolao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChampionshipExternalId",
                table: "Boloes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Boloes_ChampionshipExternalId",
                table: "Boloes",
                column: "ChampionshipExternalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Boloes_ChampionshipExternalId",
                table: "Boloes");

            migrationBuilder.DropColumn(
                name: "ChampionshipExternalId",
                table: "Boloes");
        }
    }
}

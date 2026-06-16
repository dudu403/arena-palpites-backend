using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bolao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeBolaoInviteCodeNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Boloes_InviteCode",
                table: "Boloes");

            migrationBuilder.AlterColumn<string>(
                name: "InviteCode",
                table: "Boloes",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.CreateIndex(
                name: "IX_Boloes_InviteCode",
                table: "Boloes",
                column: "InviteCode",
                unique: true,
                filter: "[InviteCode] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Boloes_InviteCode",
                table: "Boloes");

            migrationBuilder.AlterColumn<string>(
                name: "InviteCode",
                table: "Boloes",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Boloes_InviteCode",
                table: "Boloes",
                column: "InviteCode",
                unique: true);
        }
    }
}

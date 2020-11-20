using Microsoft.EntityFrameworkCore.Migrations;

namespace ChapubelichBot.Migrations
{
    public partial class BetTokenhasoneUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RouletteNumbersBetToken_UserId",
                schema: "Botdb",
                table: "RouletteNumbersBetToken",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RouletteColorBetToken_UserId",
                schema: "Botdb",
                table: "RouletteColorBetToken",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RouletteColorBetToken_Users_UserId",
                schema: "Botdb",
                table: "RouletteColorBetToken",
                column: "UserId",
                principalSchema: "Botdb",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RouletteNumbersBetToken_Users_UserId",
                schema: "Botdb",
                table: "RouletteNumbersBetToken",
                column: "UserId",
                principalSchema: "Botdb",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RouletteColorBetToken_Users_UserId",
                schema: "Botdb",
                table: "RouletteColorBetToken");

            migrationBuilder.DropForeignKey(
                name: "FK_RouletteNumbersBetToken_Users_UserId",
                schema: "Botdb",
                table: "RouletteNumbersBetToken");

            migrationBuilder.DropIndex(
                name: "IX_RouletteNumbersBetToken_UserId",
                schema: "Botdb",
                table: "RouletteNumbersBetToken");

            migrationBuilder.DropIndex(
                name: "IX_RouletteColorBetToken_UserId",
                schema: "Botdb",
                table: "RouletteColorBetToken");
        }
    }
}

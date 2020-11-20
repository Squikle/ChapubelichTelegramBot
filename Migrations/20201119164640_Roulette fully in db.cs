using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChapubelichBot.Migrations
{
    public partial class Roulettefullyindb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RouletteGameSessionDataChatId",
                schema: "Botdb",
                table: "RouletteNumbersBetToken",
                newName: "RouletteGameSessionChatId");

            migrationBuilder.RenameColumn(
                name: "RouletteGameSessionDataChatId",
                schema: "Botdb",
                table: "RouletteColorBetToken",
                newName: "RouletteGameSessionChatId");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivity",
                schema: "Botdb",
                table: "RouletteGameSessions",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastActivity",
                schema: "Botdb",
                table: "RouletteGameSessions");

            migrationBuilder.RenameColumn(
                name: "RouletteGameSessionChatId",
                schema: "Botdb",
                table: "RouletteNumbersBetToken",
                newName: "RouletteGameSessionDataChatId");

            migrationBuilder.RenameColumn(
                name: "RouletteGameSessionChatId",
                schema: "Botdb",
                table: "RouletteColorBetToken",
                newName: "RouletteGameSessionDataChatId");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChapubelichBot.Migrations
{
    public partial class CrocodileGameSessionUpdate1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Started",
                schema: "Botdb",
                table: "CrocodileGameSessions");

            migrationBuilder.AddColumn<int>(
                name: "Attempts",
                schema: "Botdb",
                table: "CrocodileGameSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                schema: "Botdb",
                table: "CrocodileGameSessions",
                type: "timestamp without time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attempts",
                schema: "Botdb",
                table: "CrocodileGameSessions");

            migrationBuilder.DropColumn(
                name: "StartTime",
                schema: "Botdb",
                table: "CrocodileGameSessions");

            migrationBuilder.AddColumn<bool>(
                name: "Started",
                schema: "Botdb",
                table: "CrocodileGameSessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}

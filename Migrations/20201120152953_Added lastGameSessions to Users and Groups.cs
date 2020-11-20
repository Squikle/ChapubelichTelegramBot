using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChapubelichBot.Migrations
{
    public partial class AddedlastGameSessionstoUsersandGroups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<int>>(
                name: "LastGameSessions",
                schema: "Botdb",
                table: "Users",
                type: "integer[]",
                nullable: true);

            migrationBuilder.AddColumn<List<int>>(
                name: "LastGameSessions",
                schema: "Botdb",
                table: "Groups",
                type: "integer[]",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastGameSessions",
                schema: "Botdb",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastGameSessions",
                schema: "Botdb",
                table: "Groups");
        }
    }
}

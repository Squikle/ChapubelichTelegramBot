using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChapubelichBot.Migrations
{
    public partial class LastMoneyTheftaddedtoUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastMoneyTheft",
                schema: "Botdb",
                table: "Users",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastMoneyTheft",
                schema: "Botdb",
                table: "Users");
        }
    }
}

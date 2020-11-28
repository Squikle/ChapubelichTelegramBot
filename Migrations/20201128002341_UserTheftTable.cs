using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChapubelichBot.Migrations
{
    public partial class UserTheftTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastMoneyTheft",
                schema: "Botdb",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "UserTheft",
                schema: "Botdb",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    LastMoneyTheft = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTheft", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserTheft_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Botdb",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTheft",
                schema: "Botdb");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMoneyTheft",
                schema: "Botdb",
                table: "Users",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}

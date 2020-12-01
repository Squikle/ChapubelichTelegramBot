using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ChapubelichBot.Migrations
{
    public partial class CrocodileGameSessions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CrocodileGameSessions",
                schema: "Botdb",
                columns: table => new
                {
                    ChatId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Word = table.Column<string>(type: "text", nullable: true),
                    GameMessageId = table.Column<int>(type: "integer", nullable: false),
                    HostUserId = table.Column<int>(type: "integer", nullable: true),
                    LastActivity = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrocodileGameSessions", x => x.ChatId);
                    table.ForeignKey(
                        name: "FK_CrocodileGameSessions_Users_HostUserId",
                        column: x => x.HostUserId,
                        principalSchema: "Botdb",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CrocodileGameSessionUser",
                schema: "Botdb",
                columns: table => new
                {
                    HostCandidatesUserId = table.Column<int>(type: "integer", nullable: false),
                    HostingRequestedCrocodileChatId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrocodileGameSessionUser", x => new { x.HostCandidatesUserId, x.HostingRequestedCrocodileChatId });
                    table.ForeignKey(
                        name: "FK_CrocodileGameSessionUser_CrocodileGameSessions_HostingReque~",
                        column: x => x.HostingRequestedCrocodileChatId,
                        principalSchema: "Botdb",
                        principalTable: "CrocodileGameSessions",
                        principalColumn: "ChatId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CrocodileGameSessionUser_Users_HostCandidatesUserId",
                        column: x => x.HostCandidatesUserId,
                        principalSchema: "Botdb",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrocodileGameSessions_HostUserId",
                schema: "Botdb",
                table: "CrocodileGameSessions",
                column: "HostUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrocodileGameSessionUser_HostingRequestedCrocodileChatId",
                schema: "Botdb",
                table: "CrocodileGameSessionUser",
                column: "HostingRequestedCrocodileChatId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrocodileGameSessionUser",
                schema: "Botdb");

            migrationBuilder.DropTable(
                name: "CrocodileGameSessions",
                schema: "Botdb");
        }
    }
}

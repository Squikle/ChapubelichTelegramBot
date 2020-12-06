using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChapubelichBot.Migrations
{
    public partial class CrocodileGameSession : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CrocodileGameSessions",
                schema: "Botdb",
                columns: table => new
                {
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    WordVariants = table.Column<string[]>(type: "text[]", maxLength: 50, nullable: true),
                    GameWord = table.Column<string>(type: "text", nullable: true),
                    GameMessageId = table.Column<int>(type: "integer", nullable: false),
                    GameMessageText = table.Column<string>(type: "text", nullable: true),
                    Started = table.Column<bool>(type: "boolean", nullable: false),
                    HostUserId = table.Column<int>(type: "integer", nullable: true),
                    LastActivity = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrocodileGameSessions", x => x.GroupId);
                    table.ForeignKey(
                        name: "FK_CrocodileGameSessions_Groups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "Botdb",
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CrocodileGameSessions_Users_HostUserId",
                        column: x => x.HostUserId,
                        principalSchema: "Botdb",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CrocodileHostingRegistrations",
                schema: "Botdb",
                columns: table => new
                {
                    CandidateId = table.Column<int>(type: "integer", nullable: false),
                    CrocodileGameSessionId = table.Column<long>(type: "bigint", nullable: false),
                    RegistrationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrocodileHostingRegistrations", x => new { x.CandidateId, x.CrocodileGameSessionId });
                    table.ForeignKey(
                        name: "FK_CrocodileHostingRegistrations_CrocodileGameSessions_Crocodi~",
                        column: x => x.CrocodileGameSessionId,
                        principalSchema: "Botdb",
                        principalTable: "CrocodileGameSessions",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CrocodileHostingRegistrations_Users_CandidateId",
                        column: x => x.CandidateId,
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
                name: "IX_CrocodileHostingRegistrations_CrocodileGameSessionId",
                schema: "Botdb",
                table: "CrocodileHostingRegistrations",
                column: "CrocodileGameSessionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrocodileHostingRegistrations",
                schema: "Botdb");

            migrationBuilder.DropTable(
                name: "CrocodileGameSessions",
                schema: "Botdb");
        }
    }
}

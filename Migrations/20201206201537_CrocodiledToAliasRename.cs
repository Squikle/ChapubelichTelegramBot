using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChapubelichBot.Migrations
{
    public partial class CrocodiledToAliasRename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrocodileHostCandidate",
                schema: "Botdb");

            migrationBuilder.DropTable(
                name: "CrocodileGameSessions",
                schema: "Botdb");

            migrationBuilder.CreateTable(
                name: "AliasGameSessions",
                schema: "Botdb",
                columns: table => new
                {
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    WordVariants = table.Column<string[]>(type: "text[]", maxLength: 50, nullable: true),
                    GameWord = table.Column<string>(type: "text", nullable: true),
                    GameMessageId = table.Column<int>(type: "integer", nullable: false),
                    GameMessageText = table.Column<string>(type: "text", nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    HostUserId = table.Column<int>(type: "integer", nullable: true),
                    LastActivity = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AliasGameSessions", x => x.GroupId);
                    table.ForeignKey(
                        name: "FK_AliasGameSessions_Groups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "Botdb",
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AliasGameSessions_Users_HostUserId",
                        column: x => x.HostUserId,
                        principalSchema: "Botdb",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AliasHostCandidates",
                schema: "Botdb",
                columns: table => new
                {
                    CandidateId = table.Column<int>(type: "integer", nullable: false),
                    AliasGameSessionId = table.Column<long>(type: "bigint", nullable: false),
                    RegistrationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AliasHostCandidates", x => x.CandidateId);
                    table.ForeignKey(
                        name: "FK_AliasHostCandidates_AliasGameSessions_AliasGameSessionId",
                        column: x => x.AliasGameSessionId,
                        principalSchema: "Botdb",
                        principalTable: "AliasGameSessions",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AliasHostCandidates_Users_CandidateId",
                        column: x => x.CandidateId,
                        principalSchema: "Botdb",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AliasGameSessions_HostUserId",
                schema: "Botdb",
                table: "AliasGameSessions",
                column: "HostUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AliasHostCandidates_AliasGameSessionId",
                schema: "Botdb",
                table: "AliasHostCandidates",
                column: "AliasGameSessionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AliasHostCandidates",
                schema: "Botdb");

            migrationBuilder.DropTable(
                name: "AliasGameSessions",
                schema: "Botdb");

            migrationBuilder.CreateTable(
                name: "CrocodileGameSessions",
                schema: "Botdb",
                columns: table => new
                {
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    GameMessageId = table.Column<int>(type: "integer", nullable: false),
                    GameMessageText = table.Column<string>(type: "text", nullable: true),
                    GameWord = table.Column<string>(type: "text", nullable: true),
                    HostUserId = table.Column<int>(type: "integer", nullable: true),
                    LastActivity = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    WordVariants = table.Column<string[]>(type: "text[]", maxLength: 50, nullable: true)
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
                name: "CrocodileHostCandidate",
                schema: "Botdb",
                columns: table => new
                {
                    CandidateId = table.Column<int>(type: "integer", nullable: false),
                    CrocodileGameSessionId = table.Column<long>(type: "bigint", nullable: false),
                    RegistrationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrocodileHostCandidate", x => x.CandidateId);
                    table.ForeignKey(
                        name: "FK_CrocodileHostCandidate_CrocodileGameSessions_CrocodileGameS~",
                        column: x => x.CrocodileGameSessionId,
                        principalSchema: "Botdb",
                        principalTable: "CrocodileGameSessions",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CrocodileHostCandidate_Users_CandidateId",
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
                name: "IX_CrocodileHostCandidate_CrocodileGameSessionId",
                schema: "Botdb",
                table: "CrocodileHostCandidate",
                column: "CrocodileGameSessionId");
        }
    }
}

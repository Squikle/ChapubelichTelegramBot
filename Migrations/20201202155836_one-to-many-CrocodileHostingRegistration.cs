using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChapubelichBot.Migrations
{
    public partial class onetomanyCrocodileHostingRegistration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrocodileHostingRegistrations",
                schema: "Botdb");

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
                name: "IX_CrocodileHostCandidate_CrocodileGameSessionId",
                schema: "Botdb",
                table: "CrocodileHostCandidate",
                column: "CrocodileGameSessionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrocodileHostCandidate",
                schema: "Botdb");

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
                name: "IX_CrocodileHostingRegistrations_CrocodileGameSessionId",
                schema: "Botdb",
                table: "CrocodileHostingRegistrations",
                column: "CrocodileGameSessionId");
        }
    }
}

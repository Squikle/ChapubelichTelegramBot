using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ChapubelichBot.Migrations
{
    public partial class RouletteGameSession : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RouletteGameSessions",
                schema: "Botdb",
                columns: table => new
                {
                    ChatId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameMessageId = table.Column<int>(nullable: false),
                    Resulting = table.Column<bool>(nullable: false),
                    ResultNumber = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouletteGameSessions", x => x.ChatId);
                });

            migrationBuilder.CreateTable(
                name: "RouletteColorBetToken",
                schema: "Botdb",
                columns: table => new
                {
                    RouletteGameSessionDataChatId = table.Column<long>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(nullable: false),
                    BetSum = table.Column<long>(nullable: false),
                    ChoosenColor = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouletteColorBetToken", x => new { x.RouletteGameSessionDataChatId, x.Id });
                    table.ForeignKey(
                        name: "FK_RouletteColorBetToken_RouletteGameSessions_RouletteGameSess~",
                        column: x => x.RouletteGameSessionDataChatId,
                        principalSchema: "Botdb",
                        principalTable: "RouletteGameSessions",
                        principalColumn: "ChatId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RouletteNumbersBetToken",
                schema: "Botdb",
                columns: table => new
                {
                    RouletteGameSessionDataChatId = table.Column<long>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(nullable: false),
                    BetSum = table.Column<long>(nullable: false),
                    ChoosenNumbers = table.Column<int[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouletteNumbersBetToken", x => new { x.RouletteGameSessionDataChatId, x.Id });
                    table.ForeignKey(
                        name: "FK_RouletteNumbersBetToken_RouletteGameSessions_RouletteGameSe~",
                        column: x => x.RouletteGameSessionDataChatId,
                        principalSchema: "Botdb",
                        principalTable: "RouletteGameSessions",
                        principalColumn: "ChatId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RouletteColorBetToken",
                schema: "Botdb");

            migrationBuilder.DropTable(
                name: "RouletteNumbersBetToken",
                schema: "Botdb");

            migrationBuilder.DropTable(
                name: "RouletteGameSessions",
                schema: "Botdb");
        }
    }
}

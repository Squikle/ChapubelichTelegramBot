using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ChapubelichBot.Migrations
{
    public partial class RemovedComplimentEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoyCompliments",
                schema: "Botdb");

            migrationBuilder.DropTable(
                name: "DailyReward",
                schema: "Botdb");

            migrationBuilder.DropTable(
                name: "GirlCompliments",
                schema: "Botdb");

            migrationBuilder.CreateTable(
                name: "UserDailyReward",
                schema: "Botdb",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    Rewarded = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDailyReward", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserDailyReward_Users_UserId",
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
                name: "UserDailyReward",
                schema: "Botdb");

            migrationBuilder.CreateTable(
                name: "BoyCompliments",
                schema: "Botdb",
                columns: table => new
                {
                    ComplimentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ComplimentText = table.Column<string>(type: "VARCHAR", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoyCompliments", x => x.ComplimentId);
                });

            migrationBuilder.CreateTable(
                name: "DailyReward",
                schema: "Botdb",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Rewarded = table.Column<bool>(type: "boolean", nullable: false),
                    Stage = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyReward", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_DailyReward_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Botdb",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GirlCompliments",
                schema: "Botdb",
                columns: table => new
                {
                    ComplimentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ComplimentText = table.Column<string>(type: "VARCHAR", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GirlCompliments", x => x.ComplimentId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoyCompliments_ComplimentText",
                schema: "Botdb",
                table: "BoyCompliments",
                column: "ComplimentText",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GirlCompliments_ComplimentText",
                schema: "Botdb",
                table: "GirlCompliments",
                column: "ComplimentText",
                unique: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace ChapubelichBot.Migrations
{
    public partial class DailyRewardedTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyRewarded",
                schema: "Botdb",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "DailyReward",
                schema: "Botdb",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    Rewarded = table.Column<bool>(type: "boolean", nullable: false)
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyReward",
                schema: "Botdb");

            migrationBuilder.AddColumn<bool>(
                name: "DailyRewarded",
                schema: "Botdb",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}

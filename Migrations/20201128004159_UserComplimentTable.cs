using Microsoft.EntityFrameworkCore.Migrations;

namespace ChapubelichBot.Migrations
{
    public partial class UserComplimentTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComplimentSubscription",
                schema: "Botdb",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Complimented",
                schema: "Botdb",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "UserCompliment",
                schema: "Botdb",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Praised = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCompliment", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserCompliment_Users_UserId",
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
                name: "UserCompliment",
                schema: "Botdb");

            migrationBuilder.AddColumn<bool>(
                name: "ComplimentSubscription",
                schema: "Botdb",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Complimented",
                schema: "Botdb",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}

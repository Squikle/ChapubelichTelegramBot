using Microsoft.EntityFrameworkCore.Migrations;

namespace ChapubelichBot.Migrations
{
    public partial class GroupDailyPerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupDailyPerson",
                schema: "Botdb",
                columns: table => new
                {
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    RolledName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupDailyPerson", x => x.GroupId);
                    table.ForeignKey(
                        name: "FK_GroupDailyPerson_Groups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "Botdb",
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GroupDailyPerson_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Botdb",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupDailyPerson_UserId",
                schema: "Botdb",
                table: "GroupDailyPerson",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupDailyPerson",
                schema: "Botdb");
        }
    }
}

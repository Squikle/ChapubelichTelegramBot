using Microsoft.EntityFrameworkCore.Migrations;

namespace ChapubelichBot.Migrations
{
    public partial class removedUserGroupclass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserGroup",
                schema: "Botdb");

            migrationBuilder.CreateTable(
                name: "GroupUser",
                schema: "Botdb",
                columns: table => new
                {
                    GroupsGroupId = table.Column<long>(type: "bigint", nullable: false),
                    UsersUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupUser", x => new { x.GroupsGroupId, x.UsersUserId });
                    table.ForeignKey(
                        name: "FK_GroupUser_Groups_GroupsGroupId",
                        column: x => x.GroupsGroupId,
                        principalSchema: "Botdb",
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupUser_Users_UsersUserId",
                        column: x => x.UsersUserId,
                        principalSchema: "Botdb",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupUser_UsersUserId",
                schema: "Botdb",
                table: "GroupUser",
                column: "UsersUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupUser",
                schema: "Botdb");

            migrationBuilder.CreateTable(
                name: "UserGroup",
                schema: "Botdb",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    GroupId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroup", x => new { x.UserId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_UserGroup_Groups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "Botdb",
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGroup_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Botdb",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserGroup_GroupId",
                schema: "Botdb",
                table: "UserGroup",
                column: "GroupId");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ChapubelichBot.Database.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Botdb");

            migrationBuilder.CreateTable(
                name: "BoyCompliments",
                schema: "Botdb",
                columns: table => new
                {
                    ComplimentId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ComplimentText = table.Column<string>(type: "VARCHAR", nullable: false),
                    Discriminator = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoyCompliments", x => x.ComplimentId);
                });

            migrationBuilder.CreateTable(
                name: "Configurations",
                schema: "Botdb",
                columns: table => new
                {
                    Id = table.Column<bool>(nullable: false),
                    LastResetTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GirlCompliments",
                schema: "Botdb",
                columns: table => new
                {
                    ComplimentId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ComplimentText = table.Column<string>(type: "VARCHAR", nullable: false),
                    Discriminator = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GirlCompliments", x => x.ComplimentId);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                schema: "Botdb",
                columns: table => new
                {
                    GroupId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: true),
                    IsAvailable = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.GroupId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "Botdb",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Gender = table.Column<bool>(nullable: false),
                    Username = table.Column<string>(maxLength: 32, nullable: true),
                    FirstName = table.Column<string>(maxLength: 64, nullable: true),
                    Balance = table.Column<long>(nullable: false),
                    IsAvailable = table.Column<bool>(nullable: false),
                    DefaultBet = table.Column<short>(nullable: false),
                    Complimented = table.Column<bool>(nullable: false),
                    ComplimentSubscription = table.Column<bool>(nullable: false),
                    DailyRewarded = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "UserGroup",
                schema: "Botdb",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    GroupId = table.Column<long>(nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_UserGroup_GroupId",
                schema: "Botdb",
                table: "UserGroup",
                column: "GroupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoyCompliments",
                schema: "Botdb");

            migrationBuilder.DropTable(
                name: "Configurations",
                schema: "Botdb");

            migrationBuilder.DropTable(
                name: "GirlCompliments",
                schema: "Botdb");

            migrationBuilder.DropTable(
                name: "UserGroup",
                schema: "Botdb");

            migrationBuilder.DropTable(
                name: "Groups",
                schema: "Botdb");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "Botdb");
        }
    }
}

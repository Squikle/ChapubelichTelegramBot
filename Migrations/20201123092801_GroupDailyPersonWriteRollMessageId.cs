using Microsoft.EntityFrameworkCore.Migrations;

namespace ChapubelichBot.Migrations
{
    public partial class GroupDailyPersonWriteRollMessageId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RollMessageId",
                schema: "Botdb",
                table: "GroupDailyPerson",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RollMessageId",
                schema: "Botdb",
                table: "GroupDailyPerson");
        }
    }
}

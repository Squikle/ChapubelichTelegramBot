namespace ChapubelichBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class manytomanychangedremovedusergroup : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("Botdb.UserGroups", "GroupId", "Botdb.Groups");
            DropForeignKey("Botdb.UserGroups", "UserId", "Botdb.Users");
            DropIndex("Botdb.UserGroups", new[] { "UserId" });
            DropIndex("Botdb.UserGroups", new[] { "GroupId" });
            CreateTable(
                "Botdb.UserGroups",
                c => new
                {
                    User_UserId = c.Int(nullable: false),
                    Group_GroupId = c.Long(nullable: false),
                })
                .PrimaryKey(t => new { t.User_UserId, t.Group_GroupId })
                .ForeignKey("Botdb.Users", t => t.User_UserId, cascadeDelete: true)
                .ForeignKey("Botdb.Groups", t => t.Group_GroupId, cascadeDelete: true)
                .Index(t => t.User_UserId)
                .Index(t => t.Group_GroupId);

            DropTable("Botdb.UserGroups");
        }
        
        public override void Down()
        {
            /*CreateTable(
                "Botdb.UserGroups",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        GroupId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.GroupId });
            
            DropForeignKey("Botdb.UserGroups", "Group_GroupId", "Botdb.Groups");
            DropForeignKey("Botdb.UserGroups", "User_UserId", "Botdb.Users");
            DropIndex("Botdb.UserGroups", new[] { "Group_GroupId" });
            DropIndex("Botdb.UserGroups", new[] { "User_UserId" });
            DropTable("Botdb.UserGroups");
            CreateIndex("Botdb.UserGroups", "GroupId");
            CreateIndex("Botdb.UserGroups", "UserId");
            AddForeignKey("Botdb.UserGroups", "UserId", "Botdb.Users", "UserId", cascadeDelete: true);
            AddForeignKey("Botdb.UserGroups", "GroupId", "Botdb.Groups", "GroupId", cascadeDelete: true);*/
        }
    }
}

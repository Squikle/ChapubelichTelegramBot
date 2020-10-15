namespace ChapubelichBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class deleteinternalid : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("Botdb.UserGroups", "GroupId", "Botdb.Groups");
            DropForeignKey("Botdb.UserGroups", "UserId", "Botdb.Users");
            DropIndex("Botdb.Groups", new[] { "GroupId" });
            DropIndex("Botdb.UserGroups", new[] { "GroupId" });
            DropIndex("Botdb.Users", new[] { "UserId" });
            DropPrimaryKey("Botdb.Groups");
            DropPrimaryKey("Botdb.UserGroups");
            DropPrimaryKey("Botdb.Users");
            AlterColumn("Botdb.UserGroups", "GroupId", c => c.Long(nullable: false));
            AddPrimaryKey("Botdb.Groups", "GroupId");
            AddPrimaryKey("Botdb.UserGroups", new[] { "UserId", "GroupId" });
            AddPrimaryKey("Botdb.Users", "UserId");
            CreateIndex("Botdb.UserGroups", "GroupId");
            AddForeignKey("Botdb.UserGroups", "GroupId", "Botdb.Groups", "GroupId", cascadeDelete: true);
            AddForeignKey("Botdb.UserGroups", "UserId", "Botdb.Users", "UserId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            AddColumn("Botdb.Users", "Id", c => c.Int(nullable: false, identity: true));
            AddColumn("Botdb.Groups", "Id", c => c.Int(nullable: false, identity: true));
            DropForeignKey("Botdb.UserGroups", "UserId", "Botdb.Users");
            DropForeignKey("Botdb.UserGroups", "GroupId", "Botdb.Groups");
            DropIndex("Botdb.UserGroups", new[] { "GroupId" });
            DropPrimaryKey("Botdb.Users");
            DropPrimaryKey("Botdb.UserGroups");
            DropPrimaryKey("Botdb.Groups");
            AlterColumn("Botdb.UserGroups", "GroupId", c => c.Int(nullable: false));
            AddPrimaryKey("Botdb.Users", "Id");
            AddPrimaryKey("Botdb.UserGroups", new[] { "UserId", "GroupId" });
            AddPrimaryKey("Botdb.Groups", "Id");
            CreateIndex("Botdb.Users", "UserId", unique: true);
            CreateIndex("Botdb.UserGroups", "GroupId");
            CreateIndex("Botdb.Groups", "GroupId", unique: true);
            AddForeignKey("Botdb.UserGroups", "UserId", "Botdb.Users", "Id", cascadeDelete: true);
            AddForeignKey("Botdb.UserGroups", "GroupId", "Botdb.Groups", "Id", cascadeDelete: true);
        }
    }
}

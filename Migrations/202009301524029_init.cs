namespace Chapubelich.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Botdb.Groups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        GroupId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.GroupId, unique: true);
            
            CreateTable(
                "Botdb.UserGroups",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        GroupId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.GroupId })
                .ForeignKey("Botdb.Groups", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("Botdb.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.GroupId);
            
            CreateTable(
                "Botdb.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Gender = c.Boolean(),
                        Username = c.String(),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserId, unique: true);
            
        }
        
        public override void Down()
        {
            DropForeignKey("Botdb.UserGroups", "UserId", "Botdb.Users");
            DropForeignKey("Botdb.UserGroups", "GroupId", "Botdb.Groups");
            DropIndex("Botdb.Users", new[] { "UserId" });
            DropIndex("Botdb.UserGroups", new[] { "GroupId" });
            DropIndex("Botdb.UserGroups", new[] { "UserId" });
            DropIndex("Botdb.Groups", new[] { "GroupId" });
            DropTable("Botdb.Users");
            DropTable("Botdb.UserGroups");
            DropTable("Botdb.Groups");
        }
    }
}

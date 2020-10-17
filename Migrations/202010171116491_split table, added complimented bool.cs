namespace ChapubelichBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class splittableaddedcomplimentedbool : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Botdb.UsersDetails",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        Gender = c.Boolean(nullable: false),
                        DefaultBet = c.Short(nullable: false),
                        Complimented = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("Botdb.Users", t => t.UserId)
                .Index(t => t.UserId);

            DropColumn("Botdb.Users", "Gender");
            DropColumn("Botdb.Users", "DefaultBet");
        }
        
        public override void Down()
        {
            AddColumn("Botdb.Users", "DefaultBet", c => c.Short(nullable: false));
            AddColumn("Botdb.Users", "Gender", c => c.Boolean(nullable: false));
            DropForeignKey("Botdb.UsersDetails", "UserId", "Botdb.Users");
            DropIndex("Botdb.UsersDetails", new[] { "UserId" });
            DropTable("Botdb.UsersDetails");
        }
    }
}

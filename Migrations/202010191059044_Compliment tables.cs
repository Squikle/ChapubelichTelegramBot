namespace ChapubelichBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Complimenttables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Botdb.BoyCompliments",
                c => new
                    {
                        ComplimentId = c.Int(nullable: false, identity: true),
                        ComplimentText = c.String(nullable: false, maxLength: 8000),
                    })
                .PrimaryKey(t => t.ComplimentId)
                .Index(t => t.ComplimentText, unique: true);
            
            CreateTable(
                "Botdb.GirlCompliments",
                c => new
                    {
                        ComplimentId = c.Int(nullable: false, identity: true),
                        ComplimentText = c.String(nullable: false, maxLength: 8000),
                    })
                .PrimaryKey(t => t.ComplimentId)
                .Index(t => t.ComplimentText, unique: true);
            
        }
        
        public override void Down()
        {
            DropIndex("Botdb.GirlCompliments", new[] { "ComplimentText" });
            DropIndex("Botdb.BoyCompliments", new[] { "ComplimentText" });
            DropTable("Botdb.GirlCompliments");
            DropTable("Botdb.BoyCompliments");
        }
    }
}

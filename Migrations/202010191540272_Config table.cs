namespace ChapubelichBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configtable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Botdb.Configurations",
                c => new
                    {
                        Id = c.Boolean(nullable: false),
                        DailyReset = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("Botdb.Configurations");
        }
    }
}

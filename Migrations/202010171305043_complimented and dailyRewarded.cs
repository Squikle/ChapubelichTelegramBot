namespace ChapubelichBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class complimentedanddailyRewarded : DbMigration
    {
        public override void Up()
        {
            AddColumn("Botdb.Users", "Complimented", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("Botdb.Users", "DailyRewarded", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("Botdb.Users", "DailyRewarded");
            DropColumn("Botdb.Users", "Complimented");
        }
    }
}

namespace ChapubelichBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DailyResetchangedtodatetime : DbMigration
    {
        public override void Up()
        {
            AddColumn("Botdb.Configurations", "LastResetTime", c => c.DateTime(nullable: false, defaultValue: DateTime.Now));
            DropColumn("Botdb.Configurations", "DailyReset");
        }
        
        public override void Down()
        {
            AddColumn("Botdb.Configurations", "DailyReset", c => c.Boolean(nullable: false, defaultValue: false));
            DropColumn("Botdb.Configurations", "LastResetTime");
        }
    }
}

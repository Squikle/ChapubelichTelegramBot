namespace ChapubelichBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addeddefaultbet : DbMigration
    {
        public override void Up()
        {
            AddColumn("Botdb.Users", "DefaultBet", c => c.Int(nullable: false, defaultValue: 50));
        }
        
        public override void Down()
        {
            DropColumn("Botdb.Users", "DefaultBet");
        }
    }
}

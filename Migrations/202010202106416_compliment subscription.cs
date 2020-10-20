namespace ChapubelichBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class complimentsubscription : DbMigration
    {
        public override void Up()
        {
            AddColumn("Botdb.Users", "ComplimentSubscription", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("Botdb.Users", "ComplimentSubscription");
        }
    }
}

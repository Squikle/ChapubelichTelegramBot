namespace ChapubelichBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedBalance : DbMigration
    {
        public override void Up()
        {
            AddColumn("Botdb.Users", "Balance", c => c.Int(nullable: false, defaultValue: 0));
        }
        
        public override void Down()
        {
            DropColumn("Botdb.Users", "Balance");
        }
    }
}

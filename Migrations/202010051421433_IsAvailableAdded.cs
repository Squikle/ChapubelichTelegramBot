namespace Chapubelich.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsAvailableAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("Botdb.Users", "IsAvailable", c => c.Boolean(nullable: false, defaultValue: true));
        }
        
        public override void Down()
        {
            DropColumn("Botdb.Users", "IsAvailable");
        }
    }
}

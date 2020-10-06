namespace Chapubelich.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedFirstName : DbMigration
    {
        public override void Up()
        {
            AddColumn("Botdb.Users", "FirstName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("Botdb.Users", "FirstName");
        }
    }
}

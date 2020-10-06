namespace Chapubelich.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedIsAvailableforgroupsandchangedorderforusers : DbMigration
    {
        public override void Up()
        {
            AddColumn("Botdb.Groups", "IsAvailable", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Botdb.Groups", "IsAvailable");
        }
    }
}

namespace ChapubelichBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class notnullgender : DbMigration
    {
        public override void Up()
        {
            AlterColumn("Botdb.Users", "Gender", c => c.Boolean(nullable: false, defaultValue: true));
        }
        
        public override void Down()
        {
            AlterColumn("Botdb.Users", "Gender", c => c.Boolean());
        }
    }
}

namespace ChapubelichBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Balanceofusershasbeenconvertedtolong : DbMigration
    {
        public override void Up()
        {
            AlterColumn("Botdb.Users", "Balance", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("Botdb.Users", "Balance", c => c.Int(nullable: false));
        }
    }
}

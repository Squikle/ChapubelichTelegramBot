namespace ChapubelichBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changedsizesofUsercolumns : DbMigration
    {
        public override void Up()
        {
            AlterColumn("Botdb.Users", "Username", c => c.String(maxLength: 32));
            AlterColumn("Botdb.Users", "FirstName", c => c.String(maxLength: 64));
            AlterColumn("Botdb.Users", "DefaultBet", c => c.Short(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("Botdb.Users", "DefaultBet", c => c.Int(nullable: false));
            AlterColumn("Botdb.Users", "FirstName", c => c.String());
            AlterColumn("Botdb.Users", "Username", c => c.String());
        }
    }
}

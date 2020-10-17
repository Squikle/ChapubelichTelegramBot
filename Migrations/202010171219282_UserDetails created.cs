namespace ChapubelichBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserDetailscreated : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Botdb.UsersDetails",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        Gender = c.Boolean(nullable: false),
                        DefaultBet = c.Short(nullable: false),
                        Complimented = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.UserId);

            Sql("ALTER TABLE \"Botdb\".\"UsersDetails\" ALTER \"Complimented\" SET DEFAULT false; ");

            Sql("INSERT INTO \"Botdb\".\"UsersDetails\" (\"UserId\", \"Gender\", \"DefaultBet\") SELECT \"UserId\", \"Gender\", \"DefaultBet\" FROM \"Botdb\".\"Users\"; ");

            Sql("ALTER TABLE \"Botdb\".\"Users\" DROP COLUMN \"Gender\", DROP COLUMN \"DefaultBet\";");
        }
        
        public override void Down()
        {
            DropTable("Botdb.UsersDetails");
        }
    }
}

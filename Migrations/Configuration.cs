namespace ChapubelichBot.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<ChapubelichBot.Database.ChapubelichdbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            this.ContextKey = "ChapubelichBot.Migrations.Configuration";
        }

        protected override void Seed(ChapubelichBot.Database.ChapubelichdbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
        }
    }
}

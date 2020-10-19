using ChapubelichBot.Database.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Migrations.History;

namespace ChapubelichBot.Database
{
    class ChapubelichdbContext : DbContext
    {
#if (DEBUG)
        public ChapubelichdbContext() : base("Server=localhost;Port=5432;Username=postgres;Password=1234;Database=ChapubelichTestdb;")
        { }
#else
        public ChapubelichdbContext() : base("ChapubelichConnectionString")
        { }
#endif

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Botdb");

            /*modelBuilder.Entity<UserGroup>().HasKey(x => new { x.UserId, x.GroupId });

            modelBuilder.Entity<UserGroup>()
            .HasRequired<User>(sc => sc.User)
            .WithMany(s => s.UserGroup)
            .HasForeignKey(sc => sc.UserId);

            modelBuilder.Entity<UserGroup>()
            .HasRequired<Group>(sc => sc.Group)
            .WithMany(s => s.UserGroup)
            .HasForeignKey(sc => sc.GroupId);*/

            modelBuilder.Entity<User>()
            .Property(p => p.UserId)
            .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            modelBuilder.Entity<Group>()
            .Property(p => p.GroupId)
            .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<BoyCompliment> BoyCompliments {get; set;}
        public DbSet<GirlCompliment> GirlCompliments { get; set; }
    }
}
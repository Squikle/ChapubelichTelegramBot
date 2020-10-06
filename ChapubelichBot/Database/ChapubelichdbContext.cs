using Chapubelich.Database.Models;
using System.Data.Entity;

namespace Chapubelich.Database
{
    public class ChapubelichdbContext : DbContext
    {
        public ChapubelichdbContext() : base("ChapubelichConnectionString")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Botdb");

            modelBuilder.Entity<UserGroup>().HasKey(x => new { x.UserId, x.GroupId });

            modelBuilder.Entity<UserGroup>()
            .HasRequired<User>(sc => sc.User)
            .WithMany(s => s.UserGroup)
            .HasForeignKey(sc => sc.UserId);

            modelBuilder.Entity<UserGroup>()
            .HasRequired<Group>(sc => sc.Group)
            .WithMany(s => s.UserGroup)
            .HasForeignKey(sc => sc.GroupId);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
    }
}
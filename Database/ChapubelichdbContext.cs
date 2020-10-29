using ChapubelichBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ChapubelichBot.Database
{
    class ChapubelichdbContext : DbContext
    {
        public ChapubelichdbContext()
        {
            Database.EnsureCreated();
        }
#if (DEBUG)
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = new ConfigurationBuilder().AddJsonFile($"./Init/AppSettings.json").Build();

            string connectionString = config.GetConnectionString("DebugConnection");
            string schema = config.GetValue<string>("DatabaseSchema");

            optionsBuilder.UseNpgsql(connectionString,
                x => x.MigrationsHistoryTable("__MigrationsHistory", schema));
        }
#else
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = new ConfigurationBuilder().AddJsonFile($"./Init/AppSettings.json").Build();

            string connectionString = config.GetConnectionString("ReleaseConnection");
            string schema = config.GetValue<string>("DatabaseSchema");

            optionsBuilder.UseNpgsql(connectionString,
                x => x.MigrationsHistoryTable("__MigrationsHistory", schema));
        }
#endif

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Botdb");

            modelBuilder.Entity<User>()
            .Property(p => p.UserId)
            .ValueGeneratedOnAdd();

            modelBuilder.Entity<Group>()
            .Property(p => p.GroupId)
            .ValueGeneratedOnAdd();

            modelBuilder.Entity<BoyCompliment>()
            .HasIndex(u => u.ComplimentText)
            .IsUnique();

            modelBuilder.Entity<GirlCompliment>()
            .HasIndex(u => u.ComplimentText)
            .IsUnique();

            modelBuilder.Entity<UserGroup>()
                .HasKey(k => new { k.UserId, k.GroupId });

            modelBuilder.Entity<UserGroup>()
                .HasOne(ug => ug.User)
                .WithMany(u => u.UserGroups)
                .HasForeignKey(ug => ug.UserId);

            modelBuilder.Entity<UserGroup>()
                .HasOne(ug => ug.Group)
                .WithMany(u => u.UserGroups)
                .HasForeignKey(ug => ug.GroupId);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<BoyCompliment> BoyCompliments {get; set;}
        public DbSet<GirlCompliment> GirlCompliments { get; set; }
        public DbSet<Configuration> Configurations { get; set; }
    }
}
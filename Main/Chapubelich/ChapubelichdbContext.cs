using ChapubelichBot.Types.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ChapubelichBot.Main.Chapubelich
{
    public class ChapubelichdbContext : DbContext
    {
#if (DEBUG)
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = ChapubelichClient.GetKeys().GetConnectionString("DebugConnection");
            string schema = ChapubelichClient.GetConfig().GetValue<string>("AppSettings:DatabaseSchema");

            optionsBuilder.UseNpgsql(connectionString,
                x => x.MigrationsHistoryTable("__MigrationsHistory", schema));
        }
#else
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            string connectionString = ChapubelichClient.GetKeys().GetConnectionString("ReleaseConnection");
            string schema = ChapubelichClient.GetConfig().GetValue<string>("AppSettings:DatabaseSchema");

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

            modelBuilder.Entity<RouletteGameSession>()
                .OwnsMany(gs => gs.ColorBetTokens);
            modelBuilder.Entity<RouletteGameSession>()
                .OwnsMany(gs => gs.NumberBetTokens);

            modelBuilder
                .Entity<Group>()
                .HasOne(g => g.GroupDailyPerson)
                .WithOne(gpd => gpd.Group)
                .HasForeignKey<GroupDailyPerson>(gpd => gpd.GroupId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CrocodileGameSession>()
                .HasOne(gs => gs.Host);
            modelBuilder.Entity<CrocodileGameSession>()
                .HasOne(gs => gs.Group)
                .WithOne(g => g.CrocodileGameSession);

            modelBuilder
                .Entity<CrocodileGameSession>()
                .HasMany(gs => gs.HostingCandidates)
                .WithMany(hc => hc.HostingSessionRequests)
                .UsingEntity<CrocodileHostingRegistration>(
                j => j
                    .HasOne(chr => chr.Candidate)
                    .WithMany(c => c.CrocodileHostingRegistrations)
                    .HasForeignKey(chc => chc.CandidateId),
                j => j
                    .HasOne(chr => chr.CrocodileGameSession)
                    .WithMany(gs => gs.CrocodileHostingRegistrations)
                    .HasForeignKey(chr => chr.CrocodileGameSessionId),
                j =>
                {
                    j.Property(chg => chg.RegistrationTime).HasDefaultValueSql("timezone('utc', now())");
                    j.ToTable("CrocodileHostingRegistrations");
                }
            );

            modelBuilder.Entity<GroupDailyPerson>().HasOne(gdp => gdp.User);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<BoyCompliment> BoyCompliments {get; set;}
        public DbSet<GirlCompliment> GirlCompliments { get; set; }
        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<RouletteGameSession> RouletteGameSessions { get; set; }
        public DbSet<CrocodileGameSession> CrocodileGameSessions { get; set; }
    }
}
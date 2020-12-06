using ChapubelichBot.Types.Entities.Alias;
using ChapubelichBot.Types.Entities.Bot;
using ChapubelichBot.Types.Entities.Groups;
using ChapubelichBot.Types.Entities.Roulette;
using ChapubelichBot.Types.Entities.Users;
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
            string schema = ChapubelichClient.GetConfig().GetValue<string>("BotSettings:DatabaseSchema");

            optionsBuilder.UseNpgsql(connectionString,
                x => x.MigrationsHistoryTable("__MigrationsHistory", schema));
        }
#else
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            string connectionString = ChapubelichClient.GetKeys().GetConnectionString("ReleaseConnection");
            string schema = ChapubelichClient.GetConfig().GetValue<string>("BotSettings:DatabaseSchema");

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

            modelBuilder.Entity<AliasGameSession>()
                .HasOne(gs => gs.Host); 
            modelBuilder.Entity<AliasGameSession>()
                .HasOne(gs => gs.Group)
                .WithOne(g => g.AliasGameSession);
            modelBuilder.Entity<AliasGameSession>()
                .HasMany(gs => gs.HostCandidates);

            modelBuilder.Entity<AliasHostCandidate>()
                .HasOne(chr => chr.Candidate);
            modelBuilder.Entity<AliasHostCandidate>()
                .HasOne(chr => chr.AliasGameSession);
            modelBuilder.Entity<AliasHostCandidate>()
                .Property(hc => hc.RegistrationTime)
                .HasDefaultValueSql("timezone('utc', now())");

            modelBuilder.Entity<GroupDailyPerson>().HasOne(gdp => gdp.User);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<RouletteGameSession> RouletteGameSessions { get; set; }
        public DbSet<AliasGameSession> AliasGameSessions { get; set; }
    }
}
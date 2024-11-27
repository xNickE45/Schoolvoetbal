using Microsoft.EntityFrameworkCore;
using Schoolvoetbalapi.Model;

namespace Schoolvoetbalapi.Data
{
    public class VoetbalContext : DbContext
    {
        public DbSet<Match> Matches { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Goal> Goals { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(
                "server=localhost;" +                     // Server name
                "port=3306;" +                            // Server port
                "user=root;" +                     // Username
                "password=;" +                 // Password
                "database=Schoolvoetbal;"       // Database name
                , Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.21-mysql") // Version
            );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Team)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TeamId);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.Creator)
                .WithMany(u => u.CreatedTeams)
                .HasForeignKey(t => t.CreatorId);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Team1)
                .WithMany(t => t.HomeMatches)
                .HasForeignKey(m => m.Team1Id);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Team2)
                .WithMany(t => t.AwayMatches)
                .HasForeignKey(m => m.Team2Id);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Referee)
                .WithMany(u => u.RefereedMatches)
                .HasForeignKey(m => m.RefereeId);

            modelBuilder.Entity<Goal>()
                .HasOne(g => g.Player)
                .WithMany(u => u.Goals)
                .HasForeignKey(g => g.PlayerId);

            modelBuilder.Entity<Goal>()
                .HasOne(g => g.Match)
                .WithMany(m => m.Goals)
                .HasForeignKey(g => g.MatchId);
        }
    }
}


using Microsoft.EntityFrameworkCore;
using Schoolvoetbalapi.Model;

namespace Schoolvoetbalapi.Data
{
    public class VoetbalContext : DbContext
    {
        public DbSet<Match> Matches { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Team> Teams { get; set; }

        public DbSet<Tourney> Tourneys { get; set; }
        public DbSet<Bet> Bets { get; set; }


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
         
        }
    }
}


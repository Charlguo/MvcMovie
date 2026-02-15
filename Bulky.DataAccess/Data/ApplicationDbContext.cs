using Bulky.Models;
using Microsoft.EntityFrameworkCore;

namespace Bulky.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Table for Movies
        public DbSet<Movie> Movies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // NEW: Seed actual Movies using the correct properties (Title, Genre, etc.)
            modelBuilder.Entity<Movie>().HasData(
                new Movie
                {
                    Id = 1,
                    Title = "The Matrix",
                    Genre = "Sci-Fi",
                    Description = "A hacker discovers the reality is a simulation.",
                    MovieScript = "Neo wakes up..."
                },
                new Movie
                {
                    Id = 2,
                    Title = "Gladiator",
                    Genre = "History",
                    Description = "A Roman general seeks revenge.",
                    MovieScript = "Maximus enters the arena..."
                },
                new Movie
                {
                    Id = 3,
                    Title = "Inception",
                    Genre = "Sci-Fi",
                    Description = "A thief steals secrets through dreams.",
                    MovieScript = "The top spins..."
                }
            );
        }
    }
}
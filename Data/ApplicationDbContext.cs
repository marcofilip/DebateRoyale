using DebateRoyale.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DebateRoyale.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Room> Rooms { get; set; }
    public DbSet<Debate> Debates { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Seed Rooms
        builder.Entity<Room>().HasData(
            new Room { Id = 1, Name = "Politics Arena", GeneralTopic = "Political Discussions" },
            new Room { Id = 2, Name = "Tech Sphere", GeneralTopic = "Technology and Future" },
            new Room { Id = 3, Name = "Philosophy Hall", GeneralTopic = "Philosophical Debates" },
            new Room { Id = 4, Name = "Pop Culture Corner", GeneralTopic = "Movies, Music, and Trends" }
        );

        // Seed Debate Topics (simple list, could be its own table)
        // For this example, topics will be managed by RoomStateService
    }
}
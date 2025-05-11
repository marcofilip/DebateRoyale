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
            new Room { Id = 1, Name = "Cultura Pop", GeneralTopic = "Film, musica, trends..." },
            new Room { Id = 2, Name = "Tecnologia", GeneralTopic = "Il futuro della tecnologia" },
            new Room { Id = 3, Name = "Filosofia", GeneralTopic = "Dibattiti filosofici" },
            new Room { Id = 4, Name = "Politica", GeneralTopic = "Discussioni politiche" }
        );
    }
}
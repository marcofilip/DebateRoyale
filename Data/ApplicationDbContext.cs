using DebateRoyale.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

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

        builder.Entity<ApplicationUser>(b =>
        {
            b.Property(u => u.Id).HasMaxLength(128); 
        });

        builder.Entity<IdentityRole>(b =>
        {
            b.Property(r => r.Id).HasMaxLength(128);
        });

        builder.Entity<Room>().HasData(
            new Room { Id = 1, Name = "Cultura Pop", GeneralTopic = "Cinema, musica, tendenze e fenomeni culturali", MaxUsers = 20 },
            new Room { Id = 2, Name = "Innovazione Tecnologica", GeneralTopic = "Nuove tecnologie, futuro digitale e impatti sociali", MaxUsers = 20 },
            new Room { Id = 3, Name = "Filosofia e Pensiero Critico", GeneralTopic = "Dibattiti su etica, esistenza e grandi domande", MaxUsers = 20 },
            new Room { Id = 4, Name = "Scienza", GeneralTopic = "Fisica, biologia, chimica, scoperte e ricerche scientifiche", MaxUsers = 20 },
            new Room { Id = 5, Name = "Attualità e Politica", GeneralTopic = "Discussioni su politica, società e attualità", MaxUsers = 20 },
            new Room { Id = 6, Name = "Domande Aperte", GeneralTopic = "Argomentazioni su temi generali e curiosità", MaxUsers = 20 }
        );
    }
}
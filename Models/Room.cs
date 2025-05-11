using System.ComponentModel.DataAnnotations;

namespace DebateRoyale.Models;

public class Room
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = "Default Room Name";

    [StringLength(200)]
    public string GeneralTopic { get; set; } = "General Discussion";
    
    public int MaxUsers { get; set; } = 0;

    public ICollection<Debate> Debates { get; set; } = new List<Debate>();
}
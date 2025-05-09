using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebateRoyale.Models;

public class Debate
{
    public int Id { get; set; }

    [Required]
    public int RoomId { get; set; }
    public Room? Room { get; set; }

    [Required]
    [StringLength(250)]
    public string SpecificTopic { get; set; } = "Specific Topic Undefined";

    public string? Debater1Id { get; set; }
    public ApplicationUser? Debater1 { get; set; }

    public string? Debater2Id { get; set; }
    public ApplicationUser? Debater2 { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    [Column(TypeName = "TEXT")]
    public string Transcript { get; set; } = "";

    public string? WinnerId { get; set; }
    public ApplicationUser? Winner { get; set; }

    [Column(TypeName = "TEXT")]
    public string? AiAnalysis { get; set; }

    public int Debater1Votes { get; set; } = 0;
    public int Debater2Votes { get; set; } = 0;

    public bool IsActive { get; set; } = false;
}
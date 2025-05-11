using Microsoft.AspNetCore.Identity;

namespace DebateRoyale.Models;

public class ApplicationUser : IdentityUser
{
    [PersonalData]
    public int Wins { get; set; } = 0;

    [PersonalData]
    public int Losses { get; set; } = 0;

    [PersonalData]
    public string? SelectedAvatar { get; set; }
}
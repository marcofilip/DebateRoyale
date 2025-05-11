// Pages/Stanza.cshtml.cs
using DebateRoyale.Data;
using DebateRoyale.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DebateRoyale.Pages;

[Authorize]
public class StanzaModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public StanzaModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Room? CurrentRoom { get; private set; }
    public string? Debater1Avatar { get; set; }
    public string? Debater2Avatar { get; set; }

    public async Task<IActionResult> OnGetAsync(int roomId)
    {
        CurrentRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId);

        if (CurrentRoom == null)
        {
            // Room not found, maybe set a TempData message and redirect
            TempData["ErrorMessage"] = "The debate room you tried to enter does not exist.";
            return RedirectToPage("/StanzeList");
        }
        return Page();
    }
}
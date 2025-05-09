// Pages/StanzeList.cshtml.cs
using DebateRoyale.Data;
using DebateRoyale.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace DebateRoyale.Pages;

[Authorize] // Only authenticated users can see the list of rooms
public class StanzeListModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public StanzeListModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Room> Rooms { get; set; } = new List<Room>();

    public async Task OnGetAsync()
    {
        Rooms = await _context.Rooms.ToListAsync();
    }
}
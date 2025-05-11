// Pages/StanzeList.cshtml.cs
using DebateRoyale.Data;
using DebateRoyale.Models;
using DebateRoyale.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace DebateRoyale.Pages;

[Authorize] // Only authenticated users can see the list of rooms
public class StanzeListModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly RoomStateService _roomStateService;

    public StanzeListModel(ApplicationDbContext context, RoomStateService roomStateService)
    {
        _context = context;
        _roomStateService = roomStateService;
    }

    public IList<RoomViewModel> Rooms { get; set; } = new List<RoomViewModel>();    public class RoomViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Default Room Name";
        public string GeneralTopic { get; set; } = "General Discussion";
        public int UserCount { get; set; } = 0;
        public int MaxUsers { get; set; } = 0;
    }    public async Task OnGetAsync()
    {
        var allRoomsFromDb = await _context.Rooms.ToListAsync();
        var activeUserCounts = _roomStateService.GetActiveUserCountsPerRoom();

        Rooms = allRoomsFromDb.Select(room => new RoomViewModel
        {
            Id = room.Id,
            Name = room.Name,
            GeneralTopic = room.GeneralTopic,
            MaxUsers = room.MaxUsers,
            UserCount = activeUserCounts.TryGetValue(room.Id, out var count) ? count : 0
        }).ToList();
    }
}
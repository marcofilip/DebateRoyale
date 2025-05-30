using DebateRoyale.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DebateRoyale.Pages
{
    [Authorize]
    public class UserStatsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserStatsModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IList<UserStatViewModel> UserStats { get; set; } = new List<UserStatViewModel>();

        public class UserStatViewModel
        {
            public string Username { get; set; } = string.Empty;
            public int Wins { get; set; }
            public int Losses { get; set; }
            public int TotalDebates { get; set; }
            public double WinPercentage { get; set; }
        }

        public async Task OnGetAsync()
        {
            var users = await _userManager.Users
                                    .OrderByDescending(u => u.Wins) 
                                    .ThenBy(u => u.Losses)       
                                    .ToListAsync();

            UserStats = users.Select(u => new UserStatViewModel
            {
                Username = u.UserName ?? "N/A",
                Wins = u.Wins,
                Losses = u.Losses,
                TotalDebates = u.Wins + u.Losses,
                WinPercentage = (u.Wins + u.Losses) > 0 ? ((double)u.Wins / (u.Wins + u.Losses)) * 100 : 0
            }).ToList();
        }
    }
}
using DebateRoyale.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DebateRoyale.Areas.Admin.Pages
{
    [Authorize(Roles = "Admin")]
    public class UsersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IList<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        public async Task OnGetAsync()
        {
            Users = await _userManager.Users.ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.UserName == User.Identity.Name)
            {
                ModelState.AddModelError(string.Empty, "Cannot delete the currently logged-in administrator.");
                Users = await _userManager.Users.ToListAsync();
                return Page();
            }


            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["StatusMessage"] = $"User {user.UserName} deleted successfully.";
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                Users = await _userManager.Users.ToListAsync(); 
                return Page();
            }
            return RedirectToPage();
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BeanScene.Web.Data;

namespace BeanScene.Web.Controllers
{
    [Authorize(Roles = "Admin")] // 🔒 Only Admins can access this controller
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager,
                               RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ✅ List all users with their roles
        public async Task<IActionResult> Users()
        {
            var users = _userManager.Users.ToList();
            var model = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? user.UserName ?? "",
                    Roles = string.Join(", ", roles)
                });
            }

            return View(model);
        }

        // ✅ Handle role assignment POST request
        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                return BadRequest("Role does not exist.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            // Remove all current roles before assigning the new one
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var result = await _userManager.AddToRoleAsync(user, role);

            if (!result.Succeeded)
                TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Description));
            else
                TempData["Message"] = $"Role '{role}' assigned to {user.Email}";

            return RedirectToAction(nameof(Users));
        }
    }

    // ✅ Simple view model for user listing
    public class UserRoleViewModel
    {
        public string UserId { get; set; } = "";
        public string Email { get; set; } = "";
        public string Roles { get; set; } = "";
    }
}

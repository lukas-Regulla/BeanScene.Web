using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BeanScene.Web.Data;
using AspNetCoreGeneratedDocument;

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

        /// <summary>
        /// GET: /Admin/Users
        /// Retrieves and displays a list of all users with their assigned roles.
        /// Useful for admin to manage user roles across the system.
        /// Admin only.
        /// </summary>
        /// <returns>View displaying all users and their roles</returns>
        public async Task<IActionResult> Users()
        {
            // Retrieve all users from the database
            var users = _userManager.Users.ToList();
            // Initialize list to hold user-role information
            var model = new List<UserRoleViewModel>();

            // Iterate through each user to fetch their assigned roles
            foreach (var user in users)
            {
                // Get all roles assigned to this user
                var roles = await _userManager.GetRolesAsync(user);
                // Create view model entry for this user with their roles
                model.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? user.UserName ?? "",
                    // Join all role names with commas for display
                    Roles = string.Join(", ", roles)
                });
            }

            return View(model);
        }

        /// <summary>
        /// POST: /Admin/AssignRole
        /// Processes role assignment to a user.
        /// Removes all existing roles from user and assigns the new role.
        /// Admin only.
        /// </summary>
        /// <param name="userId">The ID of the user to assign a role to</param>
        /// <param name="role">The name of the role to assign</param>
        /// <returns>Redirects to Users with success or error message</returns>
        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            // Validate that the specified role exists in the system
            if (!await _roleManager.RoleExistsAsync(role))
                return BadRequest("Role does not exist.");

            // Retrieve the user by ID
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            // Get all current roles for the user
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remove all existing roles before assigning the new one (one role per user)
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Assign the new role to the user
            var result = await _userManager.AddToRoleAsync(user, role);

            // Check if role assignment was successful
            if (!result.Succeeded)
                // Store error message in TempData to display on redirect
                TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Description));
            else
                // Store success message in TempData to display on redirect
                TempData["Message"] = $"Role '{role}' assigned to {user.Email}";

            // Return to users list
            return RedirectToAction(nameof(Users));
        }


        /// <summary>
        /// POST: /Admin/Delete
        /// Processes deletion of a user account from the system.
        /// Prevents admins from deleting their own accounts.
        /// Admin only.
        /// </summary>
        /// <param name="id">The ID of the user to delete</param>
        /// <returns>Redirects to Users with success or error message</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            // Validate that a user ID was provided
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            // Retrieve the user by ID
            var user = await _userManager.FindByIdAsync(id);

            // Return NotFound if user doesn't exist
            if (user == null)
            {
                return NotFound();
            }

            // Get the current logged-in admin's user ID
            var currentUserId = _userManager.GetUserId(User);

            // Prevent admin from deleting their own account
            if (user.Id == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Users));
            }

            // Attempt to delete the user
            var result = await _userManager.DeleteAsync(user);

            // Check if deletion was successful
            if (!result.Succeeded)
            {
                // Store error message in TempData to display on redirect
                TempData["ErrorMessage"] =
                    "Failed to delete user: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }
            else
            {
                // Store success message in TempData to display on redirect
                TempData["SuccessMessage"] = $"User {user.Email} deleted.";
            }

            // Return to users list
            return RedirectToAction(nameof(Users));
        }
    }


    /// <summary>
    /// View model class used to display user information and roles in the admin UI.
    /// Contains properties needed to show user list with their assigned roles.
    /// </summary>
    public class UserRoleViewModel
    {
        /// <summary>
        /// The unique identifier for the user in the system.
        /// </summary>
        public string UserId { get; set; } = "";

        /// <summary>
        /// The email address of the user (displayed as their identity).
        /// </summary>
        public string Email { get; set; } = "";

        /// <summary>
        /// Comma-separated list of roles assigned to this user.
        /// </summary>
        public string Roles { get; set; } = "";
    }
}

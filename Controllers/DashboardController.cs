using BeanScene.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeanScene.Web.Models.ViewModels;


namespace BeanScene.Web.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class DashboardController : Controller
    {
        private readonly BeanSceneContext _context;

        public DashboardController(BeanSceneContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: /Dashboard/Index
        /// Displays dashboard statistics for today's and overall reservations.
        /// Shows counts of reservations by status: Pending, Confirmed, Cancelled.
        /// Admin and Staff only.
        /// </summary>
        /// <returns>View displaying reservation statistics and metrics</returns>
        public async Task<IActionResult> Index()
        {
            // Get today's date at midnight for comparison
            var today = DateTime.Today;

            // Query all reservations scheduled for today
            var todays = await _context.Reservations
                .Where(r => r.StartTime.Date == today)
                .ToListAsync();

            // Calculate today's statistics
            // Total number of reservations scheduled for today
            int totalToday = todays.Count;
            // Count of reservations still pending confirmation
            int pendingToday = todays.Count(r => r.Status == "Pending");
            // Count of reservations already confirmed
            int confirmedToday = todays.Count(r => r.Status == "Confirmed");
            // Count of reservations that have been cancelled
            int cancelledToday = todays.Count(r => r.Status == "Cancelled");

            // Calculate overall statistics across all reservations in the system
            // Total count of all pending reservations regardless of date
            int overallPending = await _context.Reservations.CountAsync(r => r.Status == "Pending");
            // Total count of all confirmed reservations regardless of date
            int overallConfirmed = await _context.Reservations.CountAsync(r => r.Status == "Confirmed");

            // Create view model with all calculated statistics
            var model = new DashboardViewModel
            {
                TotalToday = totalToday,
                PendingToday = pendingToday,
                ConfirmedToday = confirmedToday,
                CancelledToday = cancelledToday,
                OverallPending = overallPending,
                OverallConfirmed = overallConfirmed
            };

            return View(model);
        }
    }
}

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

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            // Today's reservations
            var todays = await _context.Reservations
                .Where(r => r.StartTime.Date == today)
                .ToListAsync();

            int totalToday = todays.Count;
            int pendingToday = todays.Count(r => r.Status == "Pending");
            int confirmedToday = todays.Count(r => r.Status == "Confirmed");
            int cancelledToday = todays.Count(r => r.Status == "Cancelled");

            // Overall stats
            int overallPending = await _context.Reservations.CountAsync(r => r.Status == "Pending");
            int overallConfirmed = await _context.Reservations.CountAsync(r => r.Status == "Confirmed");

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

using BeanScene.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeanScene.Web.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class DashboardController : Controller
    {
        private readonly IReservationStatisticsService _stats;

        public DashboardController(IReservationStatisticsService stats)
        {
            _stats = stats;
        }

        /// <summary>
        /// Displays today's reservation counts (total, pending, confirmed, cancelled)
        /// and overall pending/confirmed totals. Admin and Staff only.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var model = await _stats.GetTodayStatsAsync();
            return View(model);
        }
    }
}

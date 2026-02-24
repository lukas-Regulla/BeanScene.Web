using System.Diagnostics;
using BeanScene.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BeanScene.Web.Controllers
{
    /// <summary>
    /// Controller for the home page and general site navigation.
    /// Handles the main landing page, privacy page, and error handling.
    /// No specific authorization required for this controller.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// GET: /Home/Index
        /// Displays the main home/landing page of the application.
        /// </summary>
        /// <returns>View for the home page</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// GET: /Home/Privacy
        /// Displays the privacy policy page for the application.
        /// </summary>
        /// <returns>View for the privacy policy page</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// GET: /Home/Error
        /// Displays the error page when an unhandled exception occurs.
        /// Captures the request ID for error tracking and debugging purposes.
        /// </summary>
        /// <returns>View with error details including request ID</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Create error view model with current activity ID or HTTP context trace identifier
            // This helps track which request caused the error in logs
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

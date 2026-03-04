using Microsoft.AspNetCore.Mvc;

namespace BeanScene.Web.Controllers
{
    /// <summary>
    /// Controller for managing chat functionality.
    /// Currently provides a simple index view for chat interface.
    /// No authorization required for access.
    /// </summary>
    public class ChatController : Controller
    {
        /// <summary>
        /// GET: /Chat/Index
        /// Displays the main chat interface page.
        /// </summary>
        /// <returns>View with the chat interface</returns>
        public IActionResult Index()
        {
            // Return the chat view
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace BeanScene.Web.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

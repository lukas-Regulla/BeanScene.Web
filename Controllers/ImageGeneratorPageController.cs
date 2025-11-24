using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("ImageGenerator")]
[Authorize(Roles = "Admin,Staff")]
public class ImageGeneratorPageController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }
}

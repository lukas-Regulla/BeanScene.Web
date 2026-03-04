using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("ImageGenerator")]
[Authorize(Roles = "Admin,Staff")]
/// <summary>
/// Controller for the image generator feature.
/// Provides access to an image generation interface for authorized admin and staff users.
/// Routed under /ImageGenerator path.
/// </summary>
public class ImageGeneratorPageController : Controller
{
    /// <summary>
    /// GET: /ImageGenerator/
    /// Displays the image generator interface page.
    /// Only accessible to Admin and Staff users.
    /// </summary>
    /// <returns>View with the image generator interface</returns>
    [HttpGet("")]
    public IActionResult Index()
    {
        // Return the image generator view
        return View();
    }
}

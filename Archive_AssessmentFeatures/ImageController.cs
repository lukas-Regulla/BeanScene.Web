// Controllers/ImageController.cs
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Drawing.Imaging;

[Route("api/[controller]")]
[ApiController]
public class ImageController : ControllerBase
{
    [HttpGet("generate")]
    public IActionResult Generate([FromQuery] string? text = "Hello MVC + Blazor!")
    {
        using var bmp = new Bitmap(400, 200);
        using var g = Graphics.FromImage(bmp);

        g.Clear(Color.Beige);
        using var font = new Font("Arial", 18, FontStyle.Bold);
        g.DrawString(text, font, Brushes.DarkSlateBlue, new PointF(20, 80));

        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        return File(ms.ToArray(), "image/png");
    }

    [HttpGet("drawmeacircle")]
    public IActionResult DrawMeACircle([FromQuery] int diameter = 100)
    {
        diameter = Math.Max(10, Math.Min(350, diameter));
        using var bmp = new Bitmap(diameter, diameter);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        using var pen = new Pen(Color.Blue, 4);
        g.DrawEllipse(pen, 2, 2, diameter - 4, diameter - 4);
        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        return File(ms.ToArray(), "image/png");
    }

    [HttpGet("drawfilledcircle")]
    public IActionResult DrawFilledCircle([FromQuery] int diameter = 100, [FromQuery] string colour = "Blue")
    {
        diameter = Math.Max(10, Math.Min(350, diameter));
        using var bmp = new Bitmap(diameter, diameter);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        var fillColour = Color.FromName(colour);
        using var brush = new SolidBrush(fillColour);
        g.FillEllipse(brush, 2, 2, diameter - 4, diameter - 4);
        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        return File(ms.ToArray(), "image/png");
    }
}

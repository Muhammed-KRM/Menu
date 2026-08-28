using Microsoft.AspNetCore.Mvc;

namespace QrMenu.API.Controllers;

/// <summary>
/// Yemek ve Kategori fotoğraflarını sunucuya yükleyen API.
/// wwwroot/uploads klasörüne kaydeder ve erişilebilir URL yolu döner.
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<MediaController> _logger;

    public MediaController(IWebHostEnvironment env, ILogger<MediaController> logger)
    {
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// Görsel dosyasını sunucuya yükler. JPG, JPEG, PNG, WEBP destekler. Max 5MB.
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5 MB
    public async Task<IActionResult> UploadImage([FromForm] IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Lütfen geçerli bir görsel dosyası seçin." });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "Görsel boyutu en fazla 5 MB olabilir." });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return BadRequest(new { message = "Desteklenmeyen dosya formatı. Yalnızca JPG, PNG ve WebP yükleyebilirsiniz." });

        var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadsFolder = Path.Combine(rootPath, "uploads");

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativeUrl = $"/uploads/{uniqueFileName}";
        _logger.LogInformation("Yeni görsel yüklendi: {FileName} ({Size} bytes)", uniqueFileName, file.Length);

        return Ok(new
        {
            FileName = uniqueFileName,
            ImageUrl = relativeUrl,
            Size = file.Length
        });
    }
}

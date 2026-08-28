using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QrMenu.API.Data;
using QrMenu.API.DTOs;

namespace QrMenu.API.Controllers;

/// <summary>
/// Müşterinin QR kodu okuttuğunda gördüğü public menü endpoint'i.
/// Yanıt IMemoryCache ile önbelleklenir; admin değişiklik yaptığında cache temizlenir.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<MenuController> _logger;

    internal const string MenuCacheKey = "PUBLIC_QR_MENU";

    public MenuController(
        AppDbContext context,
        IMemoryCache cache,
        ILogger<MenuController> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Tüm aktif kategorileri ve bu kategorilere ait stokta olan ürünleri döndürür.
    /// Sonuç 1 saat boyunca bellekte önbelleğe alınır.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PublicMenuResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PublicMenuResponseDto>> GetPublicMenu()
    {
        _logger.LogInformation("Public menü isteği alındı.");

        if (_cache.TryGetValue(MenuCacheKey, out PublicMenuResponseDto? cachedResponse))
        {
            _logger.LogInformation("Menü cache'ten döndürüldü.");
            return Ok(cachedResponse);
        }

        _logger.LogInformation("Menü cache'te bulunamadı, veritabanından çekiliyor.");

        var categories = await _context.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new PublicCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                ImageUrl = c.ImageUrl,
                DisplayOrder = c.DisplayOrder,
                Products = c.ProductCategories
                    .Where(pc => pc.Product.IsAvailable)
                    .OrderBy(pc => pc.DisplayOrder)
                    .Select(pc => new PublicProductDto
                    {
                        Id = pc.Product.Id,
                        Name = pc.Product.Name,
                        Description = pc.Product.Description,
                        Price = pc.Product.Price,
                        ImageUrl = pc.Product.ImageUrl,
                        Calories = pc.Product.Calories,
                        PreparationTime = pc.Product.PreparationTime
                    }).ToList()
            })
            .ToListAsync();

        var response = new PublicMenuResponseDto { Categories = categories };

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(1));

        _cache.Set(MenuCacheKey, response, cacheOptions);

        _logger.LogInformation("Menü veritabanından çekildi ve cache'e yazıldı. {CategoryCount} kategori.", categories.Count);

        return Ok(response);
    }
}

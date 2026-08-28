using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QrMenu.API.Data;
using QrMenu.API.Data.Entities;
using QrMenu.API.DTOs;

namespace QrMenu.API.Controllers;

/// <summary>
/// Admin paneli için Ürün (Yemek / İçecek) yönetim API'si.
/// Ürün ekleme, güncelleme, silme, stok açıp-kapatma (toggle) ve
/// bir ürünün birden fazla kategoriye atanmasını yönetir.
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
public class AdminProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AdminProductsController> _logger;

    public AdminProductsController(
        AppDbContext context,
        IMemoryCache cache,
        ILogger<AdminProductsController> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    private void InvalidateMenuCache()
    {
        _cache.Remove(MenuController.MenuCacheKey);
        _logger.LogInformation("Ürün değişikliği nedeniyle menü önbelleği temizlendi.");
    }

    /// <summary>
    /// Tüm ürünleri ilişkili kategorileriyle birlikte listeler.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.ProductCategories)
            .ThenInclude(pc => pc.Category)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.ImageUrl,
                p.Calories,
                p.PreparationTime,
                p.IsAvailable,
                p.CreatedAt,
                Categories = p.ProductCategories.Select(pc => new
                {
                    pc.Category.Id,
                    pc.Category.Name
                })
            })
            .ToListAsync();

        return Ok(products);
    }

    /// <summary>
    /// Tek bir ürünü bağlı olduğu kategorilerle birlikte getirir.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.ProductCategories)
            .ThenInclude(pc => pc.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound(new { message = "Ürün bulunamadı." });

        return Ok(new
        {
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.ImageUrl,
            product.Calories,
            product.PreparationTime,
            product.IsAvailable,
            CategoryIds = product.ProductCategories.Select(pc => pc.CategoryId).ToList(),
            Categories = product.ProductCategories.Select(pc => new
            {
                pc.Category.Id,
                pc.Category.Name
            })
        });
    }

    /// <summary>
    /// Yeni bir ürün oluşturur ve seçilen tüm kategorilerle Many-to-Many olarak bağlar.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductCreateUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Ürün adı boş olamaz." });

        if (dto.Price < 0)
            return BadRequest(new { message = "Fiyat negatif olamaz." });

        var product = new Product
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty,
            Price = dto.Price,
            ImageUrl = dto.ImageUrl,
            Calories = dto.Calories,
            PreparationTime = dto.PreparationTime?.Trim(),
            IsAvailable = dto.IsAvailable
        };

        // Seçilen kategorileri ata
        if (dto.CategoryIds != null && dto.CategoryIds.Count > 0)
        {
            foreach (var categoryId in dto.CategoryIds.Distinct())
            {
                product.ProductCategories.Add(new ProductCategory
                {
                    CategoryId = categoryId,
                    Product = product
                });
            }
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        InvalidateMenuCache();

        _logger.LogInformation("Yeni ürün eklendi: {ProductName} (Id: {ProductId})", product.Name, product.Id);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>
    /// Var olan ürünü ve ait olduğu kategorileri günceller.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductCreateUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Ürün adı boş olamaz." });

        if (dto.Price < 0)
            return BadRequest(new { message = "Fiyat negatif olamaz." });

        var product = await _context.Products
            .Include(p => p.ProductCategories)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound(new { message = "Ürün bulunamadı." });

        product.Name = dto.Name.Trim();
        product.Description = dto.Description?.Trim() ?? string.Empty;
        product.Price = dto.Price;
        product.ImageUrl = dto.ImageUrl;
        product.Calories = dto.Calories;
        product.PreparationTime = dto.PreparationTime?.Trim();
        product.IsAvailable = dto.IsAvailable;

        // Mevcut kategori ilişkilerini temizleyip yenilerini ekle
        product.ProductCategories.Clear();
        if (dto.CategoryIds != null && dto.CategoryIds.Count > 0)
        {
            foreach (var categoryId in dto.CategoryIds.Distinct())
            {
                product.ProductCategories.Add(new ProductCategory
                {
                    CategoryId = categoryId,
                    ProductId = product.Id
                });
            }
        }

        await _context.SaveChangesAsync();
        InvalidateMenuCache();

        _logger.LogInformation("Ürün güncellendi: {ProductName} (Id: {ProductId})", product.Name, product.Id);

        return Ok(product);
    }

    /// <summary>
    /// Tek tıkla ürünün stok durumunu (var/yok) değiştirir.
    /// </summary>
    [HttpPatch("{id:guid}/toggle-availability")]
    public async Task<IActionResult> ToggleAvailability(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound(new { message = "Ürün bulunamadı." });

        product.IsAvailable = !product.IsAvailable;
        await _context.SaveChangesAsync();
        InvalidateMenuCache();

        _logger.LogInformation("Ürün stok durumu değiştirildi: {ProductName} -> IsAvailable: {Status}", product.Name, product.IsAvailable);

        return Ok(new { product.Id, product.Name, product.IsAvailable });
    }

    /// <summary>
    /// Ürünü veritabanından siler.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound(new { message = "Ürün bulunamadı." });

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        InvalidateMenuCache();

        _logger.LogInformation("Ürün silindi: {ProductName} (Id: {ProductId})", product.Name, product.Id);

        return NoContent();
    }
}

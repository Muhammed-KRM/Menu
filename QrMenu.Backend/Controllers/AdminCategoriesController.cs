using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QrMenu.API.Data;
using QrMenu.API.Data.Entities;
using QrMenu.API.DTOs;

namespace QrMenu.API.Controllers;

/// <summary>
/// Admin paneli için Kategori yönetim API'si.
/// Kategori ekleme, listeleme, güncelleme ve silme işlemlerini yönetir.
/// Her değişiklik sonrası genel menü önbelleği temizlenir.
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
public class AdminCategoriesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AdminCategoriesController> _logger;

    public AdminCategoriesController(
        AppDbContext context,
        IMemoryCache cache,
        ILogger<AdminCategoriesController> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    private void InvalidateMenuCache()
    {
        _cache.Remove(MenuController.MenuCacheKey);
        _logger.LogInformation("Kategori değişikliği nedeniyle menü önbelleği temizlendi.");
    }

    /// <summary>
    /// Tüm kategorileri sıralama düzenine göre ve atanmış ürün sayılarıyla getirir.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.ImageUrl,
                c.DisplayOrder,
                c.IsActive,
                ProductCount = c.ProductCategories.Count
            })
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>
    /// Tek bir kategoriyi detaylarıyla getirir.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return NotFound(new { message = "Kategori bulunamadı." });

        return Ok(category);
    }

    /// <summary>
    /// Yeni bir menü kategorisi oluşturur.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryCreateUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Kategori adı boş olamaz." });

        var category = new Category
        {
            Name = dto.Name.Trim(),
            ImageUrl = dto.ImageUrl,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        InvalidateMenuCache();

        _logger.LogInformation("Yeni kategori oluşturuldu: {CategoryName} (Id: {CategoryId})", category.Name, category.Id);

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    /// <summary>
    /// Var olan bir kategoriyi günceller.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CategoryCreateUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Kategori adı boş olamaz." });

        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            return NotFound(new { message = "Kategori bulunamadı." });

        category.Name = dto.Name.Trim();
        category.ImageUrl = dto.ImageUrl;
        category.DisplayOrder = dto.DisplayOrder;
        category.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        InvalidateMenuCache();

        _logger.LogInformation("Kategori güncellendi: {CategoryName} (Id: {CategoryId})", category.Name, category.Id);

        return Ok(category);
    }

    /// <summary>
    /// Kategoriyi siler. Kategori silindiğinde ProductCategory ara tablosundaki kayıtlar da silinir.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            return NotFound(new { message = "Kategori bulunamadı." });

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        InvalidateMenuCache();

        _logger.LogInformation("Kategori silindi: {CategoryName} (Id: {CategoryId})", category.Name, category.Id);

        return NoContent();
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QrMenu.API.Data;

namespace QrMenu.API.Controllers;

/// <summary>
/// Sistem değişiklik geçmişini (Audit Logs) sunan Admin API'si.
/// Kimin, ne zaman, hangi ürün veya kategoride ne değiştirdiğini (eski/yeni JSON değerleriyle) listeler.
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
public class AdminAuditLogsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminAuditLogsController> _logger;

    public AdminAuditLogsController(AppDbContext context, ILogger<AdminAuditLogsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Denetim loglarını sayfalayarak ve isteğe bağlı filtreleyerek getirir.
    /// </summary>
    /// <param name="page">Sayfa numarası (varsayılan 1)</param>
    /// <param name="pageSize">Sayfa başına kayıt (varsayılan 20)</param>
    /// <param name="actionType">Filtre: CREATE, UPDATE, DELETE, TOGGLE_STOCK</param>
    /// <param name="entityName">Filtre: Product, Category, Media</param>
    /// <param name="search">Açıklama veya kullanıcı adında serbest metin araması</param>
    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? actionType = null,
        [FromQuery] string? entityName = null,
        [FromQuery] string? search = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(actionType))
            query = query.Where(a => a.ActionType == actionType.Trim());

        if (!string.IsNullOrWhiteSpace(entityName))
            query = query.Where(a => a.EntityName == entityName.Trim());

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(a => a.Description.Contains(s) || (a.UserId != null && a.UserId.Contains(s)));
        }

        var totalCount = await query.CountAsync();

        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        _logger.LogInformation("Audit logs listelendi. Toplam {TotalCount} kayıttan {Page}. sayfa getirildi.", totalCount, page);

        return Ok(new
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            Data = logs
        });
    }
}

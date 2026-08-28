using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using QrMenu.API.Data.Entities;

namespace QrMenu.API.Data;

/// <summary>
/// Uygulamanın ana veritabanı bağlam sınıfı.
/// SaveChangesAsync override edilerek tüm ekleme, güncelleme ve silme işlemleri
/// otomatik olarak AuditLogs tablosuna kaydedilir.
/// </summary>
public class AppDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Many-to-Many: Bileşik birincil anahtar
        modelBuilder.Entity<ProductCategory>()
            .HasKey(pc => new { pc.ProductId, pc.CategoryId });

        modelBuilder.Entity<ProductCategory>()
            .HasOne(pc => pc.Product)
            .WithMany(p => p.ProductCategories)
            .HasForeignKey(pc => pc.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProductCategory>()
            .HasOne(pc => pc.Category)
            .WithMany(c => c.ProductCategories)
            .HasForeignKey(pc => pc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Performans indeksleri
        modelBuilder.Entity<AuditLog>().HasIndex(a => a.Timestamp);
        modelBuilder.Entity<AuditLog>().HasIndex(a => a.ActionType);
        modelBuilder.Entity<AuditLog>().HasIndex(a => a.EntityName);
        modelBuilder.Entity<Category>().HasIndex(c => c.DisplayOrder);
        modelBuilder.Entity<Product>().HasIndex(p => p.IsAvailable);

        // Fiyat hassasiyeti
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);
    }

    /// <summary>
    /// SaveChangesAsync override: Değişiklikleri veritabanına yazmadan önce
    /// AuditEntry nesnelerini toplar, yazdıktan sonra AuditLog kayıtlarını oluşturur.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = CollectAuditEntries();
        var result = await base.SaveChangesAsync(cancellationToken);
        await PersistAuditEntriesAsync(auditEntries, cancellationToken);
        return result;
    }

    /// <summary>
    /// ChangeTracker üzerindeki değişiklikleri tarar ve her biri için bir AuditEntry oluşturur.
    /// AuditLog varlıkları bu taramadan dışlanır (sonsuz döngü önlenir).
    /// </summary>
    private List<AuditEntry> CollectAuditEntries()
    {
        ChangeTracker.DetectChanges();
        var entries = new List<AuditEntry>();

        var httpContext = _httpContextAccessor?.HttpContext;
        var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var endpoint = httpContext?.Request?.Path.Value ?? "";
        var userId = httpContext?.User?.Identity?.Name ?? "Admin";

        foreach (var entry in ChangeTracker.Entries())
        {
            // AuditLog kayıtlarını ve değişmemiş varlıkları atla
            if (entry.Entity is AuditLog
                || entry.State == EntityState.Detached
                || entry.State == EntityState.Unchanged)
            {
                continue;
            }

            var auditEntry = new AuditEntry(entry)
            {
                EntityName = entry.Entity.GetType().Name,
                IpAddress = ipAddress,
                Endpoint = endpoint,
                UserId = userId
            };

            entries.Add(auditEntry);

            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;

                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[propertyName] = property.CurrentValue!;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.ActionType = "CREATE";
                        auditEntry.NewValues[propertyName] = property.CurrentValue!;
                        break;

                    case EntityState.Deleted:
                        auditEntry.ActionType = "DELETE";
                        auditEntry.OldValues[propertyName] = property.OriginalValue!;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditEntry.ActionType = "UPDATE";
                            auditEntry.OldValues[propertyName] = property.OriginalValue!;
                            auditEntry.NewValues[propertyName] = property.CurrentValue!;
                        }
                        break;
                }
            }
        }

        return entries;
    }

    /// <summary>
    /// Toplanan AuditEntry nesnelerini AuditLog varlıklarına dönüştürüp veritabanına yazar.
    /// </summary>
    private async Task PersistAuditEntriesAsync(
        List<AuditEntry> auditEntries,
        CancellationToken cancellationToken)
    {
        if (auditEntries.Count == 0) return;

        foreach (var auditEntry in auditEntries)
        {
            AuditLogs.Add(auditEntry.ToAuditLog());
        }

        await base.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// ChangeTracker değişikliklerini geçici olarak paketleyen yardımcı sınıf.
/// SaveChangesAsync tamamlandıktan sonra AuditLog varlığına dönüştürülür.
/// </summary>
public class AuditEntry
{
    public EntityEntry Entry { get; }
    public string UserId { get; set; } = "Admin";
    public string EntityName { get; set; } = "";
    public string ActionType { get; set; } = "";
    public string? IpAddress { get; set; }
    public string? Endpoint { get; set; }
    public Dictionary<string, object> KeyValues { get; } = new();
    public Dictionary<string, object> OldValues { get; } = new();
    public Dictionary<string, object> NewValues { get; } = new();

    public AuditEntry(EntityEntry entry)
    {
        Entry = entry;
    }

    /// <summary>
    /// Bu geçici kaydı kalıcı AuditLog veritabanı varlığına dönüştürür.
    /// </summary>
    public AuditLog ToAuditLog()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        return new AuditLog
        {
            UserId = UserId,
            ActionType = ActionType,
            EntityName = EntityName,
            EntityId = KeyValues.Count > 0
                ? JsonSerializer.Serialize(KeyValues, jsonOptions)
                : null,
            OldValuesJson = OldValues.Count == 0
                ? null
                : JsonSerializer.Serialize(OldValues, jsonOptions),
            NewValuesJson = NewValues.Count == 0
                ? null
                : JsonSerializer.Serialize(NewValues, jsonOptions),
            IpAddress = IpAddress,
            Endpoint = Endpoint,
            Timestamp = DateTime.UtcNow,
            Description = $"{EntityName} üzerinde {ActionType} işlemi yapıldı."
        };
    }
}

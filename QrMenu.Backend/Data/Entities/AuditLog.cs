namespace QrMenu.API.Data.Entities;

/// <summary>
/// Sistemdeki tüm CRUD işlemlerinin ve kullanıcı hareketlerinin
/// geriye dönük izlenebilmesi için kaydedilen denetim logu varlığı.
/// Her ekleme, güncelleme ve silme işlemi otomatik olarak bu tabloya yazılır.
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>İşlemi gerçekleştiren kullanıcı veya admin kimliği.</summary>
    public string? UserId { get; set; } = "Admin";

    /// <summary>Yapılan işlem türü: CREATE, UPDATE, DELETE, TOGGLE_STOCK, UPLOAD.</summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>Etkilenen varlığın tipi: Product, Category, Media.</summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>Etkilenen kaydın benzersiz kimliği.</summary>
    public string? EntityId { get; set; }

    /// <summary>Etkilenen kaydın insan okunabilir başlığı (Örn: "Trüflü Burger").</summary>
    public string? EntityTitle { get; set; }

    /// <summary>Değişiklik öncesi alan değerlerinin JSON temsili.</summary>
    public string? OldValuesJson { get; set; }

    /// <summary>Değişiklik sonrası alan değerlerinin JSON temsili.</summary>
    public string? NewValuesJson { get; set; }

    /// <summary>İnsan tarafından okunabilir işlem açıklaması.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>İşlemi yapan istemcinin IP adresi.</summary>
    public string? IpAddress { get; set; }

    /// <summary>Çağrılan API endpoint yolu.</summary>
    public string? Endpoint { get; set; }

    /// <summary>İşlemin gerçekleştiği zaman (UTC).</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

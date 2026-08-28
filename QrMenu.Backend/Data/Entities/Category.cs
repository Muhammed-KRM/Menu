namespace QrMenu.API.Data.Entities;

/// <summary>
/// Menü kategori varlığı (Örn: Burgerler, İçecekler, Tatlılar).
/// Bir kategori birden fazla ürüne sahip olabilir (Many-to-Many via ProductCategory).
/// </summary>
public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Kategori adı (Örn: "Başlangıçlar", "Ana Yemekler").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Kategori ikonu veya görseli için URL yolu.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Menüdeki görüntüleme sırası (küçük değer = önce gösterilir).</summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>Kategori müşteri menüsünde aktif olarak gösteriliyor mu?</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Bu kategoriye atanmış ürünler (Many-to-Many navigasyon).</summary>
    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
}

namespace QrMenu.API.Data.Entities;

/// <summary>
/// Menüdeki bir ürün (yemek/içecek) varlığı.
/// Bir ürün birden fazla kategoride yer alabilir (Many-to-Many via ProductCategory).
/// </summary>
public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Ürün adı (Örn: "Trüflü Burger").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Ürün açıklaması, içerik veya malzeme bilgisi.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Ürün fiyatı (TL cinsinden).</summary>
    public decimal Price { get; set; }

    /// <summary>Ürün görseli için URL yolu.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Kalori bilgisi (opsiyonel).</summary>
    public int? Calories { get; set; }

    /// <summary>Tahmini hazırlanma süresi (Örn: "15 dk").</summary>
    public string? PreparationTime { get; set; }

    /// <summary>Ürün stokta mı? false ise müşteri menüsünde gösterilmez.</summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>Kaydın oluşturulma tarihi (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Bu ürünün ait olduğu kategoriler (Many-to-Many navigasyon).</summary>
    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
}

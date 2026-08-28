namespace QrMenu.API.DTOs;

// =============================================================================
// PUBLIC MENÜ DTO'LARI (Müşteri QR Menü Endpoint'i)
// =============================================================================

/// <summary>Müşteriye sunulan tam menü yanıtı.</summary>
public class PublicMenuResponseDto
{
    public List<PublicCategoryDto> Categories { get; set; } = new();
}

/// <summary>Müşteri menüsündeki kategori bilgisi.</summary>
public class PublicCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public List<PublicProductDto> Products { get; set; } = new();
}

/// <summary>Müşteri menüsündeki ürün bilgisi (hassas alanlar dışlanır).</summary>
public class PublicProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public int? Calories { get; set; }
    public string? PreparationTime { get; set; }
}

// =============================================================================
// ADMİN CRUD DTO'LARI (Kategori ve Ürün Yönetimi)
// =============================================================================

/// <summary>Kategori oluşturma ve güncelleme DTO'su.</summary>
public class CategoryCreateUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Ürün oluşturma ve güncelleme DTO'su.
/// CategoryIds alanı ile bir ürün birden fazla kategoriye atanabilir.
/// </summary>
public class ProductCreateUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public int? Calories { get; set; }
    public string? PreparationTime { get; set; }
    public bool IsAvailable { get; set; } = true;

    /// <summary>Ürünün atanacağı kategori kimliklerinin listesi (çoklu seçim).</summary>
    public List<Guid> CategoryIds { get; set; } = new();
}

namespace QrMenu.API.Data.Entities;

/// <summary>
/// Product ile Category arasındaki Many-to-Many ilişkiyi sağlayan
/// ara (junction/bridge) tablosu. Bir ürün birden fazla kategoride,
/// bir kategori de birden fazla ürüne sahip olabilir.
/// </summary>
public class ProductCategory
{
    /// <summary>İlişkili ürünün kimliği.</summary>
    public Guid ProductId { get; set; }

    /// <summary>İlişkili ürün navigasyon özelliği.</summary>
    public Product Product { get; set; } = null!;

    /// <summary>İlişkili kategorinin kimliği.</summary>
    public Guid CategoryId { get; set; }

    /// <summary>İlişkili kategori navigasyon özelliği.</summary>
    public Category Category { get; set; } = null!;

    /// <summary>Bu ürünün ilgili kategori içindeki görüntüleme sırası.</summary>
    public int DisplayOrder { get; set; } = 0;
}

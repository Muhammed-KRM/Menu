using Microsoft.EntityFrameworkCore;
using QrMenu.API.Data.Entities;

namespace QrMenu.API.Data;

/// <summary>
/// Veritabanı ilk kez oluşturulduğunda örnek restoran menü verilerini yükleyen seed sınıfı.
/// Kategoriler, yemekler ve çoka-çok (Many-to-Many) eşleşmeleri oluşturur.
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync())
            return; // Zaten veri var

        // 1. Kategoriler
        var catBaslangic = new Category
        {
            Name = "Başlangıçlar & Mezeler",
            ImageUrl = "https://images.unsplash.com/photo-1541529086526-db283c563270?w=300&auto=format&fit=crop&q=80",
            DisplayOrder = 1,
            IsActive = true
        };

        var catAnaYemek = new Category
        {
            Name = "Ana Yemekler",
            ImageUrl = "https://images.unsplash.com/photo-1544025162-d76694265947?w=300&auto=format&fit=crop&q=80",
            DisplayOrder = 2,
            IsActive = true
        };

        var catBurger = new Category
        {
            Name = "Burgerler & Dürümler",
            ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=300&auto=format&fit=crop&q=80",
            DisplayOrder = 3,
            IsActive = true
        };

        var catSpesiyal = new Category
        {
            Name = "Şefin Spesiyalleri",
            ImageUrl = "https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=300&auto=format&fit=crop&q=80",
            DisplayOrder = 4,
            IsActive = true
        };

        var catTatli = new Category
        {
            Name = "Tatlılar",
            ImageUrl = "https://images.unsplash.com/photo-1551024709-8f23befc6f87?w=300&auto=format&fit=crop&q=80",
            DisplayOrder = 5,
            IsActive = true
        };

        var catIcecek = new Category
        {
            Name = "İçecekler",
            ImageUrl = "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?w=300&auto=format&fit=crop&q=80",
            DisplayOrder = 6,
            IsActive = true
        };

        context.Categories.AddRange(catBaslangic, catAnaYemek, catBurger, catSpesiyal, catTatli, catIcecek);

        // 2. Ürünler
        var prodHumus = new Product
        {
            Name = "Pastırmalı Sıcak Humus",
            Description = "Tereyağında sotelenmiş enfes Kayseri pastırması, tahin, zeytinyağı ve taze çıtır lavaş ile.",
            Price = 195.00m,
            ImageUrl = "https://images.unsplash.com/photo-1577906096429-f73c2c312435?w=600&auto=format&fit=crop&q=80",
            Calories = 420,
            PreparationTime = "10-12 dk",
            IsAvailable = true
        };

        var prodKofte = new Product
        {
            Name = "Izgara Kasap Köfte",
            Description = "Özel baharat karışımı ile yoğrulmuş 200g dana ızgara köfte, közlenmiş biber, domates ve tereyağlı pilav eşliğinde.",
            Price = 340.00m,
            ImageUrl = "https://images.unsplash.com/photo-1529042410759-befb1204b468?w=600&auto=format&fit=crop&q=80",
            Calories = 680,
            PreparationTime = "15-18 dk",
            IsAvailable = true
        };

        var prodAntrikot = new Product
        {
            Name = "Kuru Dinlendirilmiş Dana Antrikot",
            Description = "28 gün dry-aged dinlendirilmiş 280g antrikot, trüflü patates püresi ve fırınlanmış sarımsaklı tereyağı ile.",
            Price = 580.00m,
            ImageUrl = "https://images.unsplash.com/photo-1558030006-450675393462?w=600&auto=format&fit=crop&q=80",
            Calories = 790,
            PreparationTime = "20-25 dk",
            IsAvailable = true
        };

        var prodBurger = new Product
        {
            Name = "Trüflü Gurme Burger",
            Description = "180g dana burger köftesi, karamelize soğan, eritilmiş cheddar, füme et, trüf mayonez ve çıtır baharatlı patates.",
            Price = 360.00m,
            ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=600&auto=format&fit=crop&q=80",
            Calories = 890,
            PreparationTime = "15 dk",
            IsAvailable = true
        };

        var prodSanSebastian = new Product
        {
            Name = "San Sebastian Cheesecake",
            Description = "İçi akışkan, hafif karamelize kabuklu orijinal İspanyol usulü cheesecake, ılık Belçika çikolatası sosu ile.",
            Price = 180.00m,
            ImageUrl = "https://images.unsplash.com/photo-1533134242443-d4fd215305ad?w=600&auto=format&fit=crop&q=80",
            Calories = 480,
            PreparationTime = "5 dk",
            IsAvailable = true
        };

        var prodLimonata = new Product
        {
            Name = "Taze Ev Yapımı Nane Limonata",
            Description = "Taze sıkılmış limon suyu, bal, taze nane yaprakları ve buz ile serinletici ferahlık.",
            Price = 95.00m,
            ImageUrl = "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?w=600&auto=format&fit=crop&q=80",
            Calories = 120,
            PreparationTime = "5 dk",
            IsAvailable = true
        };

        context.Products.AddRange(prodHumus, prodKofte, prodAntrikot, prodBurger, prodSanSebastian, prodLimonata);

        // 3. Many-to-Many Eşleşmeleri (Bir ürün birden fazla kategoride olabilir)
        // Antrikot: Hem "Ana Yemekler" hem de "Şefin Spesiyalleri" kategorisinde
        // Burger: Hem "Burgerler & Dürümler" hem de "Şefin Spesiyalleri" kategorisinde
        var mappings = new List<ProductCategory>
        {
            new() { Product = prodHumus, Category = catBaslangic, DisplayOrder = 1 },
            new() { Product = prodKofte, Category = catAnaYemek, DisplayOrder = 1 },
            new() { Product = prodAntrikot, Category = catAnaYemek, DisplayOrder = 2 },
            new() { Product = prodAntrikot, Category = catSpesiyal, DisplayOrder = 1 }, // Many-to-Many
            new() { Product = prodBurger, Category = catBurger, DisplayOrder = 1 },
            new() { Product = prodBurger, Category = catSpesiyal, DisplayOrder = 2 }, // Many-to-Many
            new() { Product = prodSanSebastian, Category = catTatli, DisplayOrder = 1 },
            new() { Product = prodLimonata, Category = catIcecek, DisplayOrder = 1 }
        };

        context.ProductCategories.AddRange(mappings);
        await context.SaveChangesAsync();
    }
}

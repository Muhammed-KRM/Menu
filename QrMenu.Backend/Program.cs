using Microsoft.EntityFrameworkCore;
using QrMenu.API.Data;

var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// Service Registration
// =============================================================================

// Controller'ları kaydet
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Türkçe karakterlerin düzgün JSON çıktısı için
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

// Swagger / OpenAPI
builder.Services.AddOpenApi();

// EF Core + SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(3)
    ));

// HTTP Context erişimi (Audit Logging'de IP ve Endpoint yakalamak için)
builder.Services.AddHttpContextAccessor();

// Bellek Önbelleği (Müşteri menü endpoint'i için)
builder.Services.AddMemoryCache();

// CORS politikası (Angular frontend'den API'ye istek atılabilmesi için)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",   // Angular dev server
                "http://localhost:5173"    // Vite fallback
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// =============================================================================
// Middleware Pipeline
// =============================================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// CORS
app.UseCors("AllowFrontend");

// HTTPS yönlendirmesi
app.UseHttpsRedirection();

// Statik dosya sunumu (wwwroot/uploads altındaki yemek görselleri)
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

// =============================================================================
// Veritabanı otomatik oluşturma (Development ortamında)
// =============================================================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();

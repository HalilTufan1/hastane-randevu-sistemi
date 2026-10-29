using HastaneRandevu.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Veritabanı: proje klasöründe hastane.db adında SQLite dosyası
builder.Services.AddDbContext<UygulamaDbContext>(options => options.UseSqlite("Data Source=hastane.db"));

var app = builder.Build();

// Veritabanını oluştur ve boşsa örnek verileri ekle
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UygulamaDbContext>();
    db.Database.EnsureCreated();
    OrnekVeri.Ekle(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

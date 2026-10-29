using HastaneRandevu.Models;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevu.Data
{
    public class UygulamaDbContext : DbContext
    {
        public UygulamaDbContext(DbContextOptions<UygulamaDbContext> options) : base(options)
        {
        }

        // Hasta, Calisan ve Yonetici aynı "Kullanicilar" tablosunda tutulur
        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<Hasta> Hastalar { get; set; }
        public DbSet<Calisan> Calisanlar { get; set; }
        public DbSet<Yonetici> Yoneticiler { get; set; }
        public DbSet<SistemYoneticisi> SistemYoneticileri { get; set; }

        public DbSet<Kurum> Kurumlar { get; set; }
        public DbSet<Sube> Subeler { get; set; }
        public DbSet<Bolum> Bolumler { get; set; }
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<UygunlukSaati> UygunlukSaatleri { get; set; }
        public DbSet<Randevu> Randevular { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Kullanici>().HasIndex(k => k.KullaniciAdi).IsUnique();
        }
    }
}

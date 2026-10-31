using HastaneRandevu.Models;

namespace HastaneRandevu.Data
{
    // Veritabanı boşsa denemek için örnek veri ekler.
    public static class OrnekVeri
    {
        public static void Ekle(UygulamaDbContext db)
        {
            if (db.Kullanicilar.Any())
                return;

            // --- Üst düzey yönetici (admin) ---
            var admin = new SistemYoneticisi { Ad = "Sistem", Soyad = "Yöneticisi", KullaniciAdi = "admin" };
            admin.SifreBelirle("admin123");
            db.Kullanicilar.Add(admin);

            // --- Kurumlar, şubeler, bölümler, hizmetler ---
            var hastane = new Kurum { Ad = "Sakarya Şifa Hastanesi", Tur = "Hastane" };
            var serdivan = new Sube { Ad = "Serdivan Şubesi", Adres = "Kemalpaşa Mah., Serdivan" };
            var adapazari = new Sube { Ad = "Adapazarı Şubesi", Adres = "Tığcılar Mah., Adapazarı" };
            hastane.Subeler.Add(serdivan);
            hastane.Subeler.Add(adapazari);

            var klinik = new Kurum { Ad = "Gülüş Diş Kliniği", Tur = "Klinik" };
            var merkez = new Sube
            {
                Ad = "Merkez",
                Adres = "Çark Cad., Adapazarı",
                AcilisSaati = new TimeSpan(10, 0, 0),
                KapanisSaati = new TimeSpan(19, 0, 0)
            };
            klinik.Subeler.Add(merkez);

            HizmetEkle(serdivan, "Dahiliye", "Genel Muayene", 20, 800);
            HizmetEkle(serdivan, "Kardiyoloji", "Kardiyoloji Muayenesi", 30, 1000);
            HizmetEkle(serdivan, "Kardiyoloji", "EKG Çekimi", 15, 400);
            HizmetEkle(serdivan, "Laboratuvar", "Kan Tahlili", 10, 300);
            HizmetEkle(serdivan, "Radyoloji", "Akciğer Röntgeni", 15, 600);
            HizmetEkle(adapazari, "Dahiliye", "Genel Muayene", 20, 800);
            HizmetEkle(adapazari, "Laboratuvar", "Kan Tahlili", 10, 300);
            HizmetEkle(merkez, "Diş Hekimliği", "Diş Muayenesi", 20, 500);
            HizmetEkle(merkez, "Diş Hekimliği", "Diş Taşı Temizliği", 40, 1200);

            db.Kurumlar.Add(hastane);
            db.Kurumlar.Add(klinik);
            db.SaveChanges();

            // --- Her kurumun kendi yöneticisi ---
            var hastaneYoneticisi = new Yonetici { Ad = "Elif", Soyad = "Demir", KullaniciAdi = "yonetici", KurumId = hastane.Id };
            hastaneYoneticisi.SifreBelirle("yonetici123");
            var klinikYoneticisi = new Yonetici { Ad = "Hakan", Soyad = "Aydın", KullaniciAdi = "klinik", KurumId = klinik.Id };
            klinikYoneticisi.SifreBelirle("klinik123");
            db.Yoneticiler.Add(hastaneYoneticisi);
            db.Yoneticiler.Add(klinikYoneticisi);
            db.SaveChanges();
        }

        // Şubede bölüm yoksa oluşturur ve bölüme hizmet ekler
        private static Hizmet HizmetEkle(Sube sube, string bolumAdi, string hizmetAdi, int sure, int ucret)
        {
            var bolum = sube.Bolumler.FirstOrDefault(b => b.Ad == bolumAdi);
            if (bolum == null)
            {
                bolum = new Bolum { Ad = bolumAdi };
                sube.Bolumler.Add(bolum);
            }
            var hizmet = new Hizmet { Ad = hizmetAdi, SureDakika = sure, Ucret = ucret };
            bolum.Hizmetler.Add(hizmet);
            return hizmet;
        }
    }
}

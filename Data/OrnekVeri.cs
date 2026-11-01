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

            var genelMuayene = HizmetEkle(serdivan, "Dahiliye", "Genel Muayene", 20, 800);
            var kardiyoMuayene = HizmetEkle(serdivan, "Kardiyoloji", "Kardiyoloji Muayenesi", 30, 1000);
            var ekg = HizmetEkle(serdivan, "Kardiyoloji", "EKG Çekimi", 15, 400);
            var kanTahlili = HizmetEkle(serdivan, "Laboratuvar", "Kan Tahlili", 10, 300);
            var rontgen = HizmetEkle(serdivan, "Radyoloji", "Akciğer Röntgeni", 15, 600);
            var genelMuayene2 = HizmetEkle(adapazari, "Dahiliye", "Genel Muayene", 20, 800);
            var kanTahlili2 = HizmetEkle(adapazari, "Laboratuvar", "Kan Tahlili", 10, 300);
            var disMuayene = HizmetEkle(merkez, "Diş Hekimliği", "Diş Muayenesi", 20, 500);
            var disTemizligi = HizmetEkle(merkez, "Diş Hekimliği", "Diş Taşı Temizliği", 40, 1200);

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

            // --- Çalışanlar (şifreleri: calisan123) ---
            var haftaIci = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
            var calisanlar = new List<Calisan>
            {
                CalisanOlustur("Ayşe", "Yılmaz", "dr.ayse", "Uzm. Dr.", "Dahiliye", serdivan, new[] { genelMuayene },
                    new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }, 9, 16),
                CalisanOlustur("Mehmet", "Kaya", "dr.mehmet", "Doç. Dr.", "Kardiyoloji", serdivan, new[] { kardiyoMuayene, ekg },
                    new[] { DayOfWeek.Tuesday, DayOfWeek.Thursday }, 9, 15),
                CalisanOlustur("Zeynep", "Arslan", "zeynep", "Laborant", "Laboratuvar", serdivan, new[] { kanTahlili },
                    haftaIci, 8, 12),
                CalisanOlustur("Can", "Öztürk", "can", "Radyoloji Teknikeri", "Radyoloji", serdivan, new[] { rontgen },
                    haftaIci, 13, 17),
                CalisanOlustur("Burak", "Şahin", "dr.burak", "Dr.", "Dahiliye", adapazari, new[] { genelMuayene2 },
                    haftaIci, 10, 16),
                CalisanOlustur("Ömer", "Tekin", "omer", "Laborant", "Laboratuvar", adapazari, new[] { kanTahlili2 },
                    haftaIci, 8, 12),
                CalisanOlustur("Selin", "Koç", "dt.selin", "Dt.", "Diş Hekimliği", merkez, new[] { disMuayene, disTemizligi },
                    new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday }, 10, 18),
            };
            db.Calisanlar.AddRange(calisanlar);
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

        private static Calisan CalisanOlustur(string ad, string soyad, string kullaniciAdi, string unvan, string uzmanlik,
            Sube sube, Hizmet[] hizmetler, DayOfWeek[] gunler, int baslangicSaati, int bitisSaati)
        {
            var calisan = new Calisan
            {
                Ad = ad,
                Soyad = soyad,
                KullaniciAdi = kullaniciAdi,
                Unvan = unvan,
                Uzmanlik = uzmanlik,
                SubeId = sube.Id,
                Sube = sube
            };
            calisan.SifreBelirle("calisan123");
            calisan.Hizmetler.AddRange(hizmetler);
            foreach (var gun in gunler)
            {
                calisan.UygunlukSaatleri.Add(new UygunlukSaati
                {
                    Gun = gun,
                    Baslangic = new TimeSpan(baslangicSaati, 0, 0),
                    Bitis = new TimeSpan(bitisSaati, 0, 0)
                });
            }
            return calisan;
        }
    }
}

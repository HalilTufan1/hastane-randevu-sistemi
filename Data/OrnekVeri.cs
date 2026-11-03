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

            // --- Hastalar (şifreleri: hasta123) ---
            string[] adlar = { "Ali", "Fatma", "Ahmet", "Zehra", "Mustafa", "Elif", "Emre", "Merve", "Hakan", "Büşra", "Murat", "Gizem", "Yusuf", "İrem", "Kerem" };
            string[] soyadlar = { "Veli", "Çelik", "Yıldız", "Aydın", "Doğan", "Kurt", "Koç", "Aslan", "Polat", "Kara", "Şimşek", "Erdem", "Özkan", "Tekin", "Bulut" };
            var hastalar = new List<Hasta>();
            for (int i = 0; i < adlar.Length; i++)
            {
                var hasta = new Hasta
                {
                    Ad = adlar[i],
                    Soyad = soyadlar[i],
                    KullaniciAdi = i == 0 ? "hasta" : "hasta" + (i + 1),
                    Telefon = "0555" + (1000000 + i * 12345)
                };
                hasta.SifreBelirle("hasta123");
                hastalar.Add(hasta);
            }
            db.Hastalar.AddRange(hastalar);
            db.SaveChanges();

            // --- Randevular: son 14 gün ve önümüzdeki 14 gün ---
            var rastgele = new Random(42);
            var randevular = new List<Randevu>();
            for (int gunFarki = -14; gunFarki <= 14; gunFarki++)
            {
                DateTime gun = DateTime.Today.AddDays(gunFarki);
                foreach (var calisan in calisanlar)
                {
                    foreach (var saat in calisan.UygunlukSaatleri.Where(u => u.Gun == gun.DayOfWeek))
                    {
                        var hizmet = calisan.Hizmetler[rastgele.Next(calisan.Hizmetler.Count)];
                        DateTime baslangic = gun + saat.Baslangic;

                        while (baslangic.AddMinutes(hizmet.SureDakika) <= gun + saat.Bitis)
                        {
                            DateTime bitis = baslangic.AddMinutes(hizmet.SureDakika);
                            var hasta = hastalar[rastgele.Next(hastalar.Count)];
                            bool hastaMusait = !randevular.Any(r => r.HastaId == hasta.Id && r.CakisiyorMu(baslangic, bitis));

                            // Her saati doldurmamak için %20 ihtimalle randevu oluştur
                            if (hastaMusait && rastgele.Next(100) < 20)
                            {
                                var randevu = new Randevu
                                {
                                    HastaId = hasta.Id,
                                    CalisanId = calisan.Id,
                                    HizmetId = hizmet.Id,
                                    Baslangic = baslangic,
                                    Bitis = bitis,
                                    Ucret = hizmet.Ucret
                                };
                                int zar = rastgele.Next(100);
                                if (gunFarki < 0)
                                {
                                    // Geçmiş randevuların çoğu onaylanmış olsun
                                    if (zar < 80) randevu.Onayla();
                                    else if (zar < 90) randevu.IptalEt();
                                    else randevu.Reddet();
                                }
                                else
                                {
                                    if (zar < 45) randevu.Onayla();
                                    else if (zar < 55) randevu.IptalEt();
                                    // geri kalanı onay bekliyor
                                }
                                randevular.Add(randevu);
                            }
                            baslangic = bitis;
                        }
                    }
                }
            }
            db.Randevular.AddRange(randevular);
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

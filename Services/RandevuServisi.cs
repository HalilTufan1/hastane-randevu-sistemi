using HastaneRandevu.Data;
using HastaneRandevu.Models;

namespace HastaneRandevu.Services
{
    // Randevu alma kuralları bu sınıfta toplandı.
    public class RandevuServisi
    {
        private readonly UygulamaDbContext _db;

        public RandevuServisi(UygulamaDbContext db)
        {
            _db = db;
        }

        // Randevu alınabiliyorsa null, alınamıyorsa hata mesajı döner.
        // calisan nesnesi Sube, Hizmetler ve UygunlukSaatleri ile birlikte yüklenmiş olmalı.
        public string? Kontrol(Hasta hasta, Calisan calisan, Hizmet hizmet, DateTime baslangic, List<Randevu> gununRandevulari)
        {
            DateTime bitis = baslangic.AddMinutes(hizmet.SureDakika);

            if (baslangic <= DateTime.Now)
                return "Geçmiş bir saate randevu alınamaz.";

            if (!calisan.Hizmetler.Any(h => h.Id == hizmet.Id))
                return "Seçilen çalışan bu hizmeti vermiyor.";

            if (calisan.Sube == null || !calisan.Sube.AcikMi(baslangic, bitis))
                return "Şube bu saatte kapalı.";

            if (!calisan.UygunMu(baslangic, bitis))
                return "Çalışan bu saatte çalışmıyor.";

            foreach (var r in gununRandevulari)
            {
                if (r.CalisanId == calisan.Id && r.CakisiyorMu(baslangic, bitis))
                    return "Çalışanın bu saatte başka bir randevusu var.";

                if (r.HastaId == hasta.Id && r.CakisiyorMu(baslangic, bitis))
                    return "Bu saatte başka bir randevunuz var.";
            }

            return null;
        }

        public List<Randevu> GununRandevulari(DateTime gun)
        {
            DateTime basla = gun.Date;
            DateTime bitir = gun.Date.AddDays(1);
            return _db.Randevular.Where(r => r.Baslangic >= basla && r.Baslangic < bitir).ToList();
        }

        // Seçilen gün için 15 dakika arayla boş saatleri bulur
        public List<DateTime> BosSaatler(Hasta hasta, Calisan calisan, Hizmet hizmet, DateTime gun)
        {
            var sonuc = new List<DateTime>();
            var randevular = GununRandevulari(gun);

            foreach (var saat in calisan.UygunlukSaatleri.Where(u => u.Gun == gun.DayOfWeek).OrderBy(u => u.Baslangic))
            {
                DateTime aday = gun.Date + saat.Baslangic;
                while (aday.AddMinutes(hizmet.SureDakika) <= gun.Date + saat.Bitis)
                {
                    if (Kontrol(hasta, calisan, hizmet, aday, randevular) == null)
                        sonuc.Add(aday);
                    aday = aday.AddMinutes(15);
                }
            }
            return sonuc;
        }

        public string? RandevuOlustur(Hasta hasta, Calisan calisan, Hizmet hizmet, DateTime baslangic, string? not)
        {
            string? hata = Kontrol(hasta, calisan, hizmet, baslangic, GununRandevulari(baslangic));
            if (hata != null)
                return hata;

            var randevu = new Randevu
            {
                HastaId = hasta.Id,
                CalisanId = calisan.Id,
                HizmetId = hizmet.Id,
                Baslangic = baslangic,
                Bitis = baslangic.AddMinutes(hizmet.SureDakika),
                Ucret = hizmet.Ucret,
                Not = not ?? ""
            };
            _db.Randevular.Add(randevu);
            _db.SaveChanges();
            return null;
        }
    }
}

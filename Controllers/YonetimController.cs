using System.Security.Claims;
using HastaneRandevu.Data;
using HastaneRandevu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevu.Controllers
{
    // Admin ve kurum yöneticisinin ortak sayfası.
    // Admin bütün kurumları, kurum yöneticisi sadece kendi kurumunu görür ve yönetir.
    [Authorize(Roles = "Admin,Yonetici")]
    public class YonetimController : Controller
    {
        private readonly UygulamaDbContext _db;

        public YonetimController(UygulamaDbContext db)
        {
            _db = db;
        }

        // ---------- Yetki kontrolü ----------

        private Kullanici AktifKullanici()
        {
            int id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return _db.Kullanicilar.First(k => k.Id == id);
        }

        // Giriş yapan kişi bu kurumu yönetebilir mi? (Admin her zaman evet, yönetici sadece kendi kurumu)
        private bool Yetkili(int kurumId)
        {
            return AktifKullanici().KurumuYonetebilirMi(kurumId);
        }

        private IActionResult YetkiYok()
        {
            TempData["Hata"] = "Bu işlem için yetkiniz yok.";
            return RedirectToAction("Index");
        }

        private List<Kurum> YonettigimKurumlar()
        {
            var kullanici = AktifKullanici();
            var kurumlar = _db.Kurumlar
                .Include(k => k.Subeler)
                .ThenInclude(s => s.Bolumler)
                .ThenInclude(b => b.Hizmetler)
                .ToList();
            return kurumlar.Where(k => kullanici.KurumuYonetebilirMi(k.Id)).ToList();
        }

        // ---------- Kurum / Şube / Bölüm / Hizmet ----------

        public IActionResult Index()
        {
            return View(YonettigimKurumlar());
        }

        [HttpPost]
        public IActionResult SubeEkle(int kurumId, string ad, string adres, TimeSpan acilis, TimeSpan kapanis)
        {
            if (!Yetkili(kurumId))
                return YetkiYok();

            var kurum = _db.Kurumlar.Include(k => k.Subeler).FirstOrDefault(k => k.Id == kurumId);
            if (kurum == null || string.IsNullOrWhiteSpace(ad))
                TempData["Hata"] = "Kurum seçin ve şube adını yazın.";
            else if (!kurum.SubeEklenebilirMi())
                TempData["Hata"] = "Kliniklerin yalnızca bir şubesi olabilir.";
            else if (kapanis <= acilis)
                TempData["Hata"] = "Kapanış saati açılış saatinden sonra olmalıdır.";
            else
            {
                kurum.Subeler.Add(new Sube { Ad = ad, Adres = adres ?? "", AcilisSaati = acilis, KapanisSaati = kapanis });
                _db.SaveChanges();
                TempData["Mesaj"] = "Şube eklendi.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult BolumEkle(int subeId, string ad)
        {
            var sube = _db.Subeler.Find(subeId);
            if (sube == null || !Yetkili(sube.KurumId))
                return YetkiYok();

            if (string.IsNullOrWhiteSpace(ad))
                TempData["Hata"] = "Bölüm adı boş olamaz.";
            else
            {
                _db.Bolumler.Add(new Bolum { SubeId = subeId, Ad = ad });
                _db.SaveChanges();
                TempData["Mesaj"] = "Bölüm eklendi.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult HizmetEkle(int bolumId, string ad, int sureDakika, int ucret)
        {
            var bolum = _db.Bolumler.Include(b => b.Sube).FirstOrDefault(b => b.Id == bolumId);
            if (bolum == null || !Yetkili(bolum.Sube!.KurumId))
                return YetkiYok();

            if (string.IsNullOrWhiteSpace(ad) || sureDakika <= 0 || ucret < 0)
                TempData["Hata"] = "Hizmet adı, süresi ve ücreti doğru girilmelidir.";
            else
            {
                _db.Hizmetler.Add(new Hizmet { BolumId = bolumId, Ad = ad, SureDakika = sureDakika, Ucret = ucret });
                _db.SaveChanges();
                TempData["Mesaj"] = "Hizmet eklendi.";
            }
            return RedirectToAction("Index");
        }

        // ---------- Çalışanlar ----------

        public IActionResult Calisanlar()
        {
            var kullanici = AktifKullanici();

            ViewBag.Subeler = _db.Subeler.Include(s => s.Kurum).ToList()
                .Where(s => kullanici.KurumuYonetebilirMi(s.KurumId)).ToList();
            ViewBag.Hizmetler = _db.Hizmetler.Include(h => h.Bolum).ThenInclude(b => b!.Sube).ToList()
                .Where(h => kullanici.KurumuYonetebilirMi(h.Bolum!.Sube!.KurumId)).ToList();

            var calisanlar = _db.Calisanlar
                .Include(c => c.Sube)
                .Include(c => c.Hizmetler)
                .ToList()
                .Where(c => kullanici.KurumuYonetebilirMi(c.Sube!.KurumId))
                .ToList();
            return View(calisanlar);
        }

        [HttpPost]
        public IActionResult CalisanEkle(string ad, string soyad, string kullaniciAdi, string sifre,
            string unvan, string uzmanlik, int subeId, List<int> hizmetIdleri)
        {
            var sube = _db.Subeler.Find(subeId);
            if (sube == null || !Yetkili(sube.KurumId))
                return YetkiYok();

            if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(kullaniciAdi) || sifre == null || sifre.Length < 6)
            {
                TempData["Hata"] = "Ad, kullanıcı adı ve en az 6 karakterlik şifre gerekli.";
                return RedirectToAction("Calisanlar");
            }
            if (_db.Kullanicilar.Any(k => k.KullaniciAdi == kullaniciAdi))
            {
                TempData["Hata"] = "Bu kullanıcı adı alınmış.";
                return RedirectToAction("Calisanlar");
            }

            var calisan = new Calisan
            {
                Ad = ad,
                Soyad = soyad ?? "",
                KullaniciAdi = kullaniciAdi,
                Unvan = unvan ?? "",
                Uzmanlik = uzmanlik ?? "",
                SubeId = subeId
            };
            calisan.SifreBelirle(sifre);

            // Sadece çalışanın şubesindeki hizmetler eklenir
            calisan.Hizmetler = _db.Hizmetler
                .Where(h => hizmetIdleri.Contains(h.Id) && h.Bolum!.SubeId == subeId)
                .ToList();

            _db.Calisanlar.Add(calisan);
            _db.SaveChanges();
            TempData["Mesaj"] = "Çalışan eklendi. Şimdi çalışma saatlerini girin.";
            return RedirectToAction("Uygunluk", new { id = calisan.Id });
        }

        private Calisan? CalisanGetir(int id)
        {
            return _db.Calisanlar
                .Include(c => c.Sube)
                .Include(c => c.Hizmetler)
                .Include(c => c.UygunlukSaatleri)
                .FirstOrDefault(c => c.Id == id);
        }

        // ---------- Çalışanın haftalık çalışma saatleri ----------

        public IActionResult Uygunluk(int id)
        {
            var calisan = CalisanGetir(id);
            if (calisan == null)
                return NotFound();
            if (!Yetkili(calisan.Sube!.KurumId))
                return YetkiYok();
            return View(calisan);
        }

        [HttpPost]
        public IActionResult UygunlukEkle(int calisanId, DayOfWeek gun, TimeSpan baslangic, TimeSpan bitis)
        {
            var calisan = CalisanGetir(calisanId);
            if (calisan == null || !Yetkili(calisan.Sube!.KurumId))
                return YetkiYok();

            if (bitis <= baslangic)
                TempData["Hata"] = "Bitiş saati başlangıçtan sonra olmalıdır.";
            else if (gun == DayOfWeek.Sunday)
                TempData["Hata"] = "Pazar günü şubeler kapalı.";
            else if (baslangic < calisan.Sube.AcilisSaati || bitis > calisan.Sube.KapanisSaati)
                TempData["Hata"] = "Saatler şubenin çalışma saatleri içinde olmalıdır.";
            else if (calisan.SaatCakisiyorMu(gun, baslangic, bitis))
                TempData["Hata"] = "Bu saat, çalışanın o günkü başka bir çalışma saatiyle çakışıyor.";
            else
            {
                _db.UygunlukSaatleri.Add(new UygunlukSaati { CalisanId = calisanId, Gun = gun, Baslangic = baslangic, Bitis = bitis });
                _db.SaveChanges();
                TempData["Mesaj"] = "Çalışma saati eklendi.";
            }
            return RedirectToAction("Uygunluk", new { id = calisanId });
        }

        [HttpPost]
        public IActionResult UygunlukSil(int id)
        {
            var saat = _db.UygunlukSaatleri.Find(id);
            if (saat == null)
                return RedirectToAction("Calisanlar");

            var calisan = CalisanGetir(saat.CalisanId);
            if (calisan == null || !Yetkili(calisan.Sube!.KurumId))
                return YetkiYok();

            _db.UygunlukSaatleri.Remove(saat);
            _db.SaveChanges();
            TempData["Mesaj"] = "Çalışma saati silindi.";
            return RedirectToAction("Uygunluk", new { id = saat.CalisanId });
        }
    }
}

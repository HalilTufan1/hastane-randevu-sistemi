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
    }
}

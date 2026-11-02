using System.Security.Claims;
using HastaneRandevu.Data;
using HastaneRandevu.Models;
using HastaneRandevu.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevu.Controllers
{
    [Authorize(Roles = "Hasta")]
    public class HastaController : Controller
    {
        private readonly UygulamaDbContext _db;
        private readonly RandevuServisi _randevuServisi;

        public HastaController(UygulamaDbContext db, RandevuServisi randevuServisi)
        {
            _db = db;
            _randevuServisi = randevuServisi;
        }

        private Hasta AktifHasta()
        {
            int id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return _db.Hastalar.First(h => h.Id == id);
        }

        // Randevularım
        public IActionResult Index()
        {
            var hasta = AktifHasta();
            var randevular = _db.Randevular
                .Include(r => r.Calisan)
                .Include(r => r.Hizmet).ThenInclude(h => h!.Bolum).ThenInclude(b => b!.Sube)
                .Where(r => r.HastaId == hasta.Id)
                .OrderByDescending(r => r.Baslangic)
                .ToList();
            return View(randevular);
        }

        // Randevu alma sayfası. Önce hizmet, sonra çalışan, sonra tarih seçilir.
        public IActionResult RandevuAl(int? hizmetId, int? calisanId, DateTime? tarih)
        {
            ViewBag.Hizmetler = _db.Hizmetler
                .Include(h => h.Bolum).ThenInclude(b => b!.Sube).ThenInclude(s => s!.Kurum)
                .ToList();
            ViewBag.HizmetId = hizmetId;
            ViewBag.CalisanId = calisanId;
            ViewBag.Tarih = tarih;
            ViewBag.Calisanlar = new List<Calisan>();
            ViewBag.BosSaatler = new List<DateTime>();

            if (hizmetId != null)
            {
                ViewBag.Calisanlar = _db.Calisanlar
                    .Include(c => c.UygunlukSaatleri)
                    .Where(c => c.Hizmetler.Any(h => h.Id == hizmetId))
                    .ToList();
            }

            if (hizmetId != null && calisanId != null && tarih != null)
            {
                var hizmet = _db.Hizmetler.Find(hizmetId);
                var calisan = CalisaniGetir(calisanId.Value);
                if (hizmet != null && calisan != null)
                {
                    ViewBag.BosSaatler = _randevuServisi.BosSaatler(AktifHasta(), calisan, hizmet, tarih.Value);
                }
            }

            return View();
        }

        [HttpPost]
        public IActionResult RandevuAl(int hizmetId, int calisanId, DateTime baslangic, string? not)
        {
            var hizmet = _db.Hizmetler.Find(hizmetId);
            var calisan = CalisaniGetir(calisanId);
            if (hizmet == null || calisan == null)
            {
                TempData["Hata"] = "Hizmet veya çalışan bulunamadı.";
                return RedirectToAction("RandevuAl");
            }

            string? hata = _randevuServisi.RandevuOlustur(AktifHasta(), calisan, hizmet, baslangic, not);
            if (hata != null)
            {
                TempData["Hata"] = hata;
                return RedirectToAction("RandevuAl", new { hizmetId, calisanId, tarih = baslangic.ToString("yyyy-MM-dd") });
            }

            TempData["Mesaj"] = "Randevu talebiniz alındı. Onaylandığında durumu güncellenecek.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Iptal(int id)
        {
            var hasta = AktifHasta();
            var randevu = _db.Randevular.FirstOrDefault(r => r.Id == id && r.HastaId == hasta.Id);

            if (randevu == null)
                TempData["Hata"] = "Randevu bulunamadı.";
            else if (randevu.Baslangic <= DateTime.Now)
                TempData["Hata"] = "Geçmiş randevu iptal edilemez.";
            else if (!randevu.IptalEt())
                TempData["Hata"] = "Bu randevu iptal edilemez.";
            else
            {
                _db.SaveChanges();
                TempData["Mesaj"] = "Randevu iptal edildi.";
            }
            return RedirectToAction("Index");
        }

        private Calisan? CalisaniGetir(int id)
        {
            return _db.Calisanlar
                .Include(c => c.Sube)
                .Include(c => c.Hizmetler)
                .Include(c => c.UygunlukSaatleri)
                .FirstOrDefault(c => c.Id == id);
        }
    }
}

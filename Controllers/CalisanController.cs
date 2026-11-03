using System.Security.Claims;
using HastaneRandevu.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevu.Controllers
{
    // Doktor ve sağlık personelinin sayfası
    [Authorize(Roles = "Calisan")]
    public class CalisanController : Controller
    {
        private readonly UygulamaDbContext _db;

        public CalisanController(UygulamaDbContext db)
        {
            _db = db;
        }

        private int AktifId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        public IActionResult Index()
        {
            int id = AktifId();
            ViewBag.Calisan = _db.Calisanlar
                .Include(c => c.Sube)
                .Include(c => c.Hizmetler)
                .Include(c => c.UygunlukSaatleri)
                .First(c => c.Id == id);

            var randevular = _db.Randevular
                .Include(r => r.Hasta)
                .Include(r => r.Hizmet)
                .Where(r => r.CalisanId == id)
                .OrderByDescending(r => r.Baslangic)
                .ToList();
            return View(randevular);
        }

        [HttpPost]
        public IActionResult Onayla(int id)
        {
            var randevu = _db.Randevular.FirstOrDefault(r => r.Id == id && r.CalisanId == AktifId());
            if (randevu != null && randevu.Onayla())
            {
                _db.SaveChanges();
                TempData["Mesaj"] = "Randevu onaylandı.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Reddet(int id)
        {
            var randevu = _db.Randevular.FirstOrDefault(r => r.Id == id && r.CalisanId == AktifId());
            if (randevu != null && randevu.Reddet())
            {
                _db.SaveChanges();
                TempData["Mesaj"] = "Randevu reddedildi.";
            }
            return RedirectToAction("Index");
        }
    }
}

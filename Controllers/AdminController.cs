using System.Security.Claims;
using HastaneRandevu.Data;
using HastaneRandevu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevu.Controllers
{
    // Sadece üst düzey yöneticinin (admin) yapabildiği işlemler:
    // kurum ekleme / düzenleme / silme, kurum yöneticisi ekleme, kullanıcı ve randevu silme.
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UygulamaDbContext _db;

        public AdminController(UygulamaDbContext db)
        {
            _db = db;
        }

        // ---------- Kurumlar ----------

        [HttpPost]
        public IActionResult KurumEkle(string ad, string tur)
        {
            if (string.IsNullOrWhiteSpace(ad))
            {
                TempData["Hata"] = "Kurum adı boş olamaz.";
            }
            else
            {
                _db.Kurumlar.Add(new Kurum { Ad = ad, Tur = tur });
                _db.SaveChanges();
                TempData["Mesaj"] = "Kurum eklendi. Şimdi bu kuruma bir yönetici atayabilirsiniz.";
            }
            return RedirectToAction("Index", "Yonetim");
        }

        public IActionResult KurumDuzenle(int id)
        {
            var kurum = _db.Kurumlar.Find(id);
            if (kurum == null)
                return NotFound();
            return View(kurum);
        }

        [HttpPost]
        public IActionResult KurumDuzenle(int id, string ad, string tur)
        {
            var kurum = _db.Kurumlar.Include(k => k.Subeler).FirstOrDefault(k => k.Id == id);
            if (kurum == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(ad))
            {
                TempData["Hata"] = "Kurum adı boş olamaz.";
                return RedirectToAction("KurumDuzenle", new { id });
            }
            if (tur == "Klinik" && kurum.Subeler.Count > 1)
            {
                TempData["Hata"] = "Birden fazla şubesi olan kurum klinik yapılamaz.";
                return RedirectToAction("KurumDuzenle", new { id });
            }

            kurum.Ad = ad;
            kurum.Tur = tur;
            _db.SaveChanges();
            TempData["Mesaj"] = "Kurum güncellendi.";
            return RedirectToAction("Index", "Yonetim");
        }

        [HttpPost]
        public IActionResult KurumSil(int id)
        {
            var kurum = _db.Kurumlar.Find(id);
            if (kurum != null)
            {
                // Kurumun şubeleri, bölümleri, hizmetleri, çalışanları, yöneticileri ve randevuları da silinir
                _db.Kurumlar.Remove(kurum);
                _db.SaveChanges();
                TempData["Mesaj"] = "Kurum ve ona bağlı tüm kayıtlar silindi.";
            }
            return RedirectToAction("Index", "Yonetim");
        }

        // ---------- Kullanıcılar (yöneticiler ve hastalar) ----------

        public IActionResult Kullanicilar()
        {
            ViewBag.Kurumlar = _db.Kurumlar.ToList();
            ViewBag.Yoneticiler = _db.Yoneticiler.Include(y => y.Kurum).ToList();
            ViewBag.Hastalar = _db.Hastalar.OrderBy(h => h.Ad).ToList();
            return View();
        }

        [HttpPost]
        public IActionResult YoneticiEkle(string ad, string soyad, string kullaniciAdi, string sifre, int kurumId)
        {
            if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(kullaniciAdi) || sifre == null || sifre.Length < 6)
            {
                TempData["Hata"] = "Ad, kullanıcı adı ve en az 6 karakterlik şifre gerekli.";
            }
            else if (_db.Kullanicilar.Any(k => k.KullaniciAdi == kullaniciAdi))
            {
                TempData["Hata"] = "Bu kullanıcı adı alınmış.";
            }
            else if (_db.Kurumlar.Find(kurumId) == null)
            {
                TempData["Hata"] = "Kurum seçin.";
            }
            else
            {
                var yonetici = new Yonetici { Ad = ad, Soyad = soyad ?? "", KullaniciAdi = kullaniciAdi, KurumId = kurumId };
                yonetici.SifreBelirle(sifre);
                _db.Yoneticiler.Add(yonetici);
                _db.SaveChanges();
                TempData["Mesaj"] = "Kurum yöneticisi eklendi.";
            }
            return RedirectToAction("Kullanicilar");
        }

        // Hasta veya kurum yöneticisi silinebilir. Admin kendini silemez.
        // (Çalışan silme Yonetim > Çalışanlar sayfasından yapılır.)
        [HttpPost]
        public IActionResult KullaniciSil(int id)
        {
            int benimId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var kullanici = _db.Kullanicilar.Find(id);

            if (kullanici == null || kullanici.Id == benimId || kullanici is SistemYoneticisi)
            {
                TempData["Hata"] = "Bu kullanıcı silinemez.";
            }
            else
            {
                _db.Kullanicilar.Remove(kullanici);
                _db.SaveChanges();
                TempData["Mesaj"] = kullanici.GorunenAd + " silindi.";
            }

            return RedirectToAction("Kullanicilar");
        }

        // ---------- Randevular ----------

        [HttpPost]
        public IActionResult RandevuSil(int id)
        {
            var randevu = _db.Randevular.Find(id);
            if (randevu != null)
            {
                _db.Randevular.Remove(randevu);
                _db.SaveChanges();
                TempData["Mesaj"] = "Randevu silindi.";
            }
            return RedirectToAction("Randevular", "Yonetim");
        }
    }
}

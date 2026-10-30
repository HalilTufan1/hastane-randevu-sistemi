using System.Security.Claims;
using HastaneRandevu.Data;
using HastaneRandevu.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace HastaneRandevu.Controllers
{
    public class HesapController : Controller
    {
        private readonly UygulamaDbContext _db;

        public HesapController(UygulamaDbContext db)
        {
            _db = db;
        }

        public IActionResult Giris()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Giris(string kullaniciAdi, string sifre)
        {
            var kullanici = _db.Kullanicilar.FirstOrDefault(k => k.KullaniciAdi == kullaniciAdi);
            if (kullanici == null || !kullanici.SifreDogruMu(sifre))
            {
                ViewBag.Hata = "Kullanıcı adı veya şifre hatalı.";
                return View();
            }

            await OturumAc(kullanici);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Kayit()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Kayit(string ad, string soyad, string kullaniciAdi, string sifre, string telefon)
        {
            if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(soyad) || string.IsNullOrWhiteSpace(kullaniciAdi))
            {
                ViewBag.Hata = "Ad, soyad ve kullanıcı adı boş bırakılamaz.";
                return View();
            }
            if (sifre == null || sifre.Length < 6)
            {
                ViewBag.Hata = "Şifre en az 6 karakter olmalıdır.";
                return View();
            }
            if (_db.Kullanicilar.Any(k => k.KullaniciAdi == kullaniciAdi))
            {
                ViewBag.Hata = "Bu kullanıcı adı alınmış.";
                return View();
            }

            var hasta = new Hasta { Ad = ad, Soyad = soyad, KullaniciAdi = kullaniciAdi, Telefon = telefon ?? "" };
            hasta.SifreBelirle(sifre);
            _db.Hastalar.Add(hasta);
            _db.SaveChanges();

            await OturumAc(hasta);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Cikis()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Yetkisiz()
        {
            return View();
        }

        private async Task OturumAc(Kullanici kullanici)
        {
            var bilgiler = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
                new Claim(ClaimTypes.Name, kullanici.GorunenAd),
                new Claim(ClaimTypes.Role, kullanici.Rol)
            };
            var kimlik = new ClaimsIdentity(bilgiler, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(kimlik));
        }
    }
}

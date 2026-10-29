using HastaneRandevu.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevu.Controllers
{
    public class HomeController : Controller
    {
        private readonly UygulamaDbContext _db;

        public HomeController(UygulamaDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var kurumlar = _db.Kurumlar
                .Include(k => k.Subeler)
                .ThenInclude(s => s.Bolumler)
                .ThenInclude(b => b.Hizmetler)
                .ToList();
            return View(kurumlar);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsWire.Data;
using NewsWire.Models;
using System.Diagnostics;

namespace NewsWire.Controllers
{
    public class HomeController : Controller
    {
        private NewsDbContext db;

        public HomeController(NewsDbContext dbContext)
        {
            db = dbContext;
        }

        public IActionResult Index()
        {
            var categories = db.Set<Category>().ToList();
            return View(categories);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult TeamMember()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
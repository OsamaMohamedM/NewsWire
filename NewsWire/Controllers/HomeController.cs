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

        [HttpPost]
        public IActionResult SaveContact(ContactUs contactUs)
        {
            if (ModelState.IsValid)
            {
                db.ContactUs.Add(contactUs);
                db.SaveChanges();
                return Content("OK");
            }
            return View(contactUs);
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult News(int id, int page = 1)
        {
            const int pageSize = 8;
            var news = db.News.Where(n => n.CategoryId == id);
            var totalNews = news.Count();
            var totalPages = (int)Math.Ceiling((double)totalNews / pageSize);

            var newsList = news.OrderByDescending(n => n.PublishedAt)
                         .Skip((page - 1) * pageSize)
                         .Take(pageSize)
                         .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CategoryId = id;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;

            return View(newsList);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
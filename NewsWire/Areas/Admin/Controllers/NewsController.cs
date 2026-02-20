using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NewsWire.Models;
using NewsWire.Services.Interfaces;

namespace NewsWire.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class NewsController : Controller
    {
        private readonly INewsService _newsService;
        private readonly ICategoryService _categoryService;

        public NewsController(INewsService newsService, ICategoryService categoryService)
        {
            _newsService = newsService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["PageTitle"] = "News Management";
            var news = await _newsService.GetAllNewsWithDetailsAsync();
            return View(news);
        }

        public async Task<IActionResult> Details(int? id)
        {
            ViewData["PageTitle"] = "News Details";
            if (id == null)
                return NotFound();

            var news = await _newsService.GetNewsWithDetailsAsync(id.Value);
            if (news == null)
                return NotFound();

            return View(news);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["PageTitle"] = "Create News";
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,ImageUrl,Topic,CategoryId")] News news)
        {
            ModelState.Remove("AuthorId");
            ModelState.Remove("Author");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                news.PublishedAt = DateTime.UtcNow;
                news.AuthorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                var success = await _newsService.CreateNewsAsync(news);
                if (success)
                {
                    TempData["SuccessMessage"] = "News article created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }

            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", news.CategoryId);
            return View(news);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            ViewData["PageTitle"] = "Edit News";
            if (id == null)
                return NotFound();

            var news = await _newsService.GetNewsByIdAsync(id.Value);
            if (news == null)
                return NotFound();

            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", news.CategoryId);
            return View(news);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,ImageUrl,PublishedAt,Topic,CategoryId,AuthorId")] News news)
        {
            if (id != news.Id)
                return NotFound();

            ModelState.Remove("Author");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                var success = await _newsService.UpdateNewsAsync(news);
                if (success)
                {
                    TempData["SuccessMessage"] = "News article updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                if (!await _newsService.NewsExistsAsync(news.Id))
                    return NotFound();
            }

            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", news.CategoryId);
            return View(news);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            ViewData["PageTitle"] = "Delete News";
            if (id == null)
                return NotFound();

            var news = await _newsService.GetNewsWithDetailsAsync(id.Value);
            if (news == null)
                return NotFound();

            return View(news);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _newsService.DeleteNewsAsync(id);
            if (success)
                TempData["SuccessMessage"] = "News article deleted successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NewsWire.Data;
using NewsWire.Models;
using System.Security.Claims;

namespace NewsWire.Controllers
{
    public class NewsController : Controller
    {
        private readonly NewsDbContext _context;
        private readonly UserManager<User> _userManager;

        public NewsController(NewsDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: News

        public IActionResult Index(int id, int page = 1)
        {
            int pageSize = 6;
            var source = _context.News
                                 .Where(n => n.CategoryId == id)
                                 .Include(n => n.Category);
            int totalItems = source.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;
            ViewBag.CategoryId = id;
            var pagedNews = source.Skip((page - 1) * pageSize)
                                  .Take(pageSize);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var viewModelList = pagedNews.Select(newsItem => new NewsDisplayViewModel
            {
                News = newsItem,
                IsOwner = (currentUserId != null && newsItem.AuthorId == currentUserId)
            }).ToList();
            return View(viewModelList);
        }

        // GET: News/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var news = _context.News
                .Include(n => n.Category)
                .FirstOrDefault(m => m.Id == id);
            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }

        // GET: News/Create
        [Authorize(Roles = "Admin,User")]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Description");
            return View();
        }

        // POST: News/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,User")]
        public IActionResult Create([Bind("Id,Title,Content,ImageUrl,PublishedAt,Topic,CategoryId")] News news)
        {
            if (ModelState.IsValid)
            {
                news.AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                news.PublishedAt = DateTime.UtcNow;

                _context.Add(news);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Article created successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Description", news.CategoryId);
            return View(news);
        }

        // GET: News/Edit/5
        [Authorize(Roles = "Admin,User")]
        public IActionResult Edit(int? id, string returnUrl = null)
        {
            if (id == null)
            {
                return NotFound();
            }

            var news = _context.News.Find(id);
            if (news == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && news.AuthorId != currentUserId)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this article.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Description", news.CategoryId);
            ViewBag.ReturnUrl = returnUrl;
            return View(news);
        }

        // POST: News/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,User")]
        public IActionResult Edit(int id, [Bind("Id,Title,Content,ImageUrl,PublishedAt,Topic,CategoryId")] News news, string returnUrl = null)
        {
            if (id != news.Id)
            {
                return NotFound();
            }

            // Check ownership before update
            var existingNews = _context.News.Find(id);
            if (existingNews == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && existingNews.AuthorId != currentUserId)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this article.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Preserve the original author
                    news.AuthorId = existingNews.AuthorId;
                    _context.Entry(existingNews).State = EntityState.Detached;
                    _context.Update(news);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Article updated successfully!";

                    // Return to specified URL or default action
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NewsExists(news.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Description", news.CategoryId);
            ViewBag.ReturnUrl = returnUrl;
            return View(news);
        }

        // GET: News/Delete/5
        [Authorize(Roles = "Admin,User")]
        public IActionResult Delete(int? id, string returnUrl = null)
        {
            if (id == null)
            {
                return NotFound();
            }

            var news = _context.News
                .Include(n => n.Category)
                .FirstOrDefault(m => m.Id == id);
            if (news == null)
            {
                return NotFound();
            }

            // Check if user can delete this news (owner or admin)
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && news.AuthorId != currentUserId)
            {
                TempData["ErrorMessage"] = "You don't have permission to delete this article.";
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(news);
        }

        // POST: News/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,User")]
        public IActionResult DeleteConfirmed(int id, string returnUrl = null)
        {
            var news = _context.News.Find(id);
            if (news != null)
            {
                // Check ownership before deletion
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!User.IsInRole("Admin") && news.AuthorId != currentUserId)
                {
                    TempData["ErrorMessage"] = "You don't have permission to delete this article.";
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction(nameof(Index));
                }

                _context.News.Remove(news);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Article deleted successfully!";
            }

            // Return to specified URL or default action
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool NewsExists(int id)
        {
            return _context.News.Any(e => e.Id == id);
        }
    }
}
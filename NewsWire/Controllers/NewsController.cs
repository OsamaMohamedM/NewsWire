using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NewsWire.Data;
using NewsWire.Models;
using NewsWire.Services;
using System.Security.Claims;

namespace NewsWire.Controllers
{
    public class NewsController : BaseController
    {
        private readonly NewsDbContext _context;
        private readonly IFileUploadService _fileUploadService;
        private readonly IFavoriteService _favoriteService;
        private readonly INewsService _newsService;

        public NewsController(
            NewsDbContext context,
            IFileUploadService fileUploadService,
            IFavoriteService favoriteService,
            INewsService newsService,
            ILogger<NewsController> logger) : base(logger)
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _favoriteService = favoriteService;
            _newsService = newsService;
        }

        public async Task<IActionResult> Index(int id, int page = 1)
        {
            try
            {
                const int pageSize = 6;
                var userId = GetCurrentUserId();

                var (totalItems, totalPages) = await _newsService.GetPaginationInfoAsync(id, pageSize);

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.HasPreviousPage = page > 1;
                ViewBag.HasNextPage = page < totalPages;
                ViewBag.CategoryId = id;

                var viewModelList = await _newsService.GetNewsByCategoryAsync(id, page, pageSize, userId);

                return View(viewModelList);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(Index));
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return HandleNullId(nameof(Details));

            try
            {
                var userId = GetCurrentUserId();
                var newsViewModel = await _newsService.GetNewsDetailsAsync(id.Value, userId);

                if (newsViewModel == null)
                    return HandleNotFound("News", id.Value);

                return View(newsViewModel);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(Details));
            }
        }

        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var categories = await _context.Categories.ToListAsync();
                ViewData["CategoryId"] = new SelectList(categories, "Id", "Name");
                return View();
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(Create));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,ImageFile,Topic,CategoryId")] News news)
        {
            ModelState.Remove("AuthorId");
            ModelState.Remove("Author");
            ModelState.Remove("Category");
            ModelState.Remove("ImageUrl");

            if (!ModelState.IsValid)
            {
                var categories = await _context.Categories.ToListAsync();
                ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", news.CategoryId);
                return View(news);
            }

            try
            {
                news.AuthorId = GetCurrentUserId();
                news.PublishedAt = DateTime.UtcNow;

                // Handle image upload using FileUploadService
                if (news.ImageFile != null && _fileUploadService.ValidateImageFile(news.ImageFile))
                {
                    var imagePath = await _fileUploadService.UploadImageAsync(news.ImageFile, "News");
                    news.ImageUrl = imagePath ?? "/assets/img/Local/default-news.jpg";
                }
                else
                {
                    news.ImageUrl = "/assets/img/Local/default-news.jpg";
                }

                _context.Add(news);
                await _context.SaveChangesAsync();

                SetSuccessMessage("Article created successfully!");
                return RedirectToAction("Index", "Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating news article");
                SetErrorMessage("Failed to create article. Please try again.");
                
                var categories = await _context.Categories.ToListAsync();
                ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", news.CategoryId);
                return View(news);
            }
        }

        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Edit(int? id, string returnUrl = null)
        {
            if (id == null)
                return HandleNullId(nameof(Edit));

            try
            {
                var news = await _context.News.FindAsync(id);
                if (news == null)
                    return HandleNotFound("News", id.Value);

                var currentUserId = GetCurrentUserId();
                if (!IsAdmin() && news.AuthorId != currentUserId)
                    return HandleUnauthorized(nameof(Edit));

                var categories = await _context.Categories.ToListAsync();
                ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", news.CategoryId);
                ViewBag.ReturnUrl = returnUrl;
                
                return View(news);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(Edit));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,ImageFile,PublishedAt,Topic,CategoryId")] News news, string returnUrl = null)
        {
            if (id != news.Id)
                return NotFound();

            try
            {
                var existingNews = await _context.News.FindAsync(id);
                if (existingNews == null)
                    return HandleNotFound("News", id);

                var currentUserId = GetCurrentUserId();
                if (!IsAdmin() && existingNews.AuthorId != currentUserId)
                    return HandleUnauthorized(nameof(Edit));

                ModelState.Remove("ImageUrl");
                ModelState.Remove("Author");
                ModelState.Remove("Category");

                if (!ModelState.IsValid)
                {
                    var categories = await _context.Categories.ToListAsync();
                    ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", news.CategoryId);
                    return View(news);
                }

                // Handle image upload
                if (news.ImageFile != null && _fileUploadService.ValidateImageFile(news.ImageFile))
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(existingNews.ImageUrl) && !existingNews.ImageUrl.Contains("default"))
                    {
                        await _fileUploadService.DeleteImageAsync(existingNews.ImageUrl);
                    }

                    // Upload new image
                    var imagePath = await _fileUploadService.UploadImageAsync(news.ImageFile, "News");
                    news.ImageUrl = imagePath ?? existingNews.ImageUrl;
                }
                else
                {
                    news.ImageUrl = existingNews.ImageUrl;
                }

                news.AuthorId = existingNews.AuthorId;
                _context.Entry(existingNews).State = EntityState.Detached;
                _context.Update(news);
                await _context.SaveChangesAsync();

                SetSuccessMessage("Article updated successfully!");

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating news article");
                SetErrorMessage("Failed to update article. Please try again.");
                
                var categories = await _context.Categories.ToListAsync();
                ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", news.CategoryId);
                return View(news);
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NewsWire.Models;
using NewsWire.Services.Interfaces;

namespace NewsWire.Controllers
{
    public class NewsController : BaseController
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly INewsService _newsService;
        private readonly ICategoryService _categoryService;

        public NewsController(
            IFileUploadService fileUploadService,
            INewsService newsService,
            ICategoryService categoryService,
            ILogger<NewsController> logger) : base(logger)
        {
            _fileUploadService = fileUploadService;
            _newsService = newsService;
            _categoryService = categoryService;
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
                var categories = await _categoryService.GetAllCategoriesAsync();
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
        [RequestSizeLimit(10485760)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10485760)]
        public async Task<IActionResult> Create([Bind("Title,Content,ImageFile,Topic,CategoryId")] News news)
        {
            ModelState.Remove("Id");
            ModelState.Remove("AuthorId");
            ModelState.Remove("Author");
            ModelState.Remove("Category");
            ModelState.Remove("ImageUrl");
            ModelState.Remove("PublishedAt");

            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", news.CategoryId);
                return View(news);
            }

            try
            {
                var userId = GetCurrentUserId();

                if (string.IsNullOrEmpty(userId))
                {
                    SetErrorMessage("Unable to identify the current user. Please log in again.");
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                news.AuthorId = userId;
                news.PublishedAt = DateTime.UtcNow;

                try
                {
                    if (news.ImageFile != null && _fileUploadService.ValidateImageFile(news.ImageFile))
                    {
                        var imagePath = await _fileUploadService.UploadImageAsync(news.ImageFile, "News");
                        news.ImageUrl = !string.IsNullOrEmpty(imagePath)
                            ? imagePath
                            : "/assets/img/Local/default-news.jpg";
                    }
                    else
                    {
                        news.ImageUrl = "/assets/img/Local/default-news.jpg";
                    }
                }
                catch (Exception imgEx)
                {
                    _logger.LogError(imgEx, "Error processing image upload");
                    news.ImageUrl = "/assets/img/Local/default-news.jpg";
                }

                var success = await _newsService.CreateNewsAsync(news);

                if (success)
                {
                    SetSuccessMessage("Article created successfully!");
                    return RedirectToAction("Index", "Profile");
                }

                SetErrorMessage("Failed to create article. Please try again.");
                var cats = await _categoryService.GetAllCategoriesAsync();
                ViewData["CategoryId"] = new SelectList(cats, "Id", "Name", news.CategoryId);
                return View(news);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Create action");
                SetErrorMessage("An unexpected error occurred.");

                var cats = await _categoryService.GetAllCategoriesAsync();
                ViewData["CategoryId"] = new SelectList(cats, "Id", "Name", news.CategoryId);
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
                var news = await _newsService.GetNewsByIdAsync(id.Value);
                if (news == null)
                    return HandleNotFound("News", id.Value);

                var currentUserId = GetCurrentUserId();
                if (!IsAdmin() && news.AuthorId != currentUserId)
                    return HandleUnauthorized(nameof(Edit));

                var categories = await _categoryService.GetAllCategoriesAsync();
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
        [RequestSizeLimit(10485760)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10485760)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,ImageFile,PublishedAt,Topic,CategoryId")] News news, string returnUrl = null)
        {
            if (id != news.Id)
                return NotFound();

            try
            {
                var existingNews = await _newsService.GetNewsByIdAsync(id);
                if (existingNews == null)
                    return HandleNotFound("News", id);

                var currentUserId = GetCurrentUserId();
                if (!IsAdmin() && existingNews.AuthorId != currentUserId)
                    return HandleUnauthorized(nameof(Edit));

                ModelState.Remove("ImageUrl");
                ModelState.Remove("Author");
                ModelState.Remove("Category");
                ModelState.Remove("AuthorId");

                if (!ModelState.IsValid)
                {
                    var categories = await _categoryService.GetAllCategoriesAsync();
                    ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", news.CategoryId);
                    return View(news);
                }

                string? oldImageUrl = existingNews.ImageUrl;

                try
                {
                    if (news.ImageFile != null && _fileUploadService.ValidateImageFile(news.ImageFile))
                    {
                        if (!string.IsNullOrEmpty(oldImageUrl) && !oldImageUrl.Contains("default"))
                        {
                            await _fileUploadService.DeleteImageAsync(oldImageUrl);
                        }

                        var imagePath = await _fileUploadService.UploadImageAsync(news.ImageFile, "News");
                        news.ImageUrl = imagePath ?? oldImageUrl ?? "/assets/img/Local/default-news.jpg";
                    }
                    else
                    {
                        news.ImageUrl = oldImageUrl ?? "/assets/img/Local/default-news.jpg";
                    }
                }
                catch (Exception imgEx)
                {
                    _logger.LogError(imgEx, "Error processing image during edit");
                    news.ImageUrl = oldImageUrl ?? "/assets/img/Local/default-news.jpg";
                }

                news.AuthorId = existingNews.AuthorId;
                news.PublishedAt = existingNews.PublishedAt;

                var success = await _newsService.UpdateNewsAsync(news);

                if (success)
                {
                    SetSuccessMessage("Article updated successfully!");

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "Profile");
                }

                SetErrorMessage("Failed to update article. Please try again.");
                var cats = await _categoryService.GetAllCategoriesAsync();
                ViewData["CategoryId"] = new SelectList(cats, "Id", "Name", news.CategoryId);
                return View(news);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(Edit));
            }
        }

        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Delete(int? id, string returnUrl = null)
        {
            if (id == null)
                return HandleNullId(nameof(Delete));

            try
            {
                var currentUserId = GetCurrentUserId();
                var newsViewModel = await _newsService.GetNewsDetailsAsync(id.Value, currentUserId);

                if (newsViewModel == null)
                    return HandleNotFound("News", id.Value);

                if (!IsAdmin() && !await _newsService.UserCanDeleteNewsAsync(currentUserId, id.Value))
                    return HandleUnauthorized(nameof(Delete));

                ViewBag.ReturnUrl = returnUrl;
                return View(newsViewModel);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(Delete));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> DeleteConfirmed(int id, string returnUrl = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var news = await _newsService.GetNewsByIdAsync(id);

                if (news == null)
                    return HandleNotFound("News", id);

                if (!IsAdmin() && !await _newsService.UserCanDeleteNewsAsync(currentUserId, id))
                    return HandleUnauthorized(nameof(Delete));

                if (!string.IsNullOrEmpty(news.ImageUrl) && !news.ImageUrl.Contains("default"))
                {
                    await _fileUploadService.DeleteImageAsync(news.ImageUrl);
                }

                var success = await _newsService.DeleteNewsAsync(id);

                if (success)
                    SetSuccessMessage("Article deleted successfully!");
                else
                    SetErrorMessage("Failed to delete article. Please try again.");

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Profile");
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(DeleteConfirmed));
            }
        }
    }
}
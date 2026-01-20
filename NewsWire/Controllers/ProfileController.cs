using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewsWire.Models;
using NewsWire.Services;
using System.Security.Claims;

namespace NewsWire.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IProfileService _profileService;
        private readonly IFavoriteService _favoriteService;
        private readonly INewsManagementService _newsManagementService;

        public ProfileController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IProfileService profileService,
            IFavoriteService favoriteService,
            INewsManagementService newsManagementService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _profileService = profileService;
            _favoriteService = favoriteService;
            _newsManagementService = newsManagementService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string tab = "profile")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return NotFound();

            var profile = await _profileService.GetProfileAsync(userId);
            if (profile == null) return NotFound();

            var statistics = await _profileService.GetProfileStatisticsAsync(userId);
            var userNews = await _profileService.GetUserNewsAsync(userId, 1, 6);
            var favoriteNews = await _profileService.GetUserFavoritesAsync(userId, 1, 6);

            var model = new UserProfileDashboardViewModel
            {
                Profile = profile,
                Statistics = statistics,
                UserNews = userNews,
                FavoriteNews = favoriteNews,
                ActiveTab = tab
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return await Index("profile");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var success = await _profileService.UpdateProfileAsync(userId, model);

            if (success)
            {
                await _signInManager.RefreshSignInAsync(await _userManager.FindByIdAsync(userId));
                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update profile. Please try again.";
            }

            return RedirectToAction(nameof(Index), new { tab = "profile" });
        }

        [HttpGet]
        public async Task<IActionResult> MyNews(int page = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userNews = await _profileService.GetUserNewsAsync(userId, page, 9);
            
            ViewBag.CurrentPage = page;
            ViewBag.PageTitle = "My Articles";
            
            return View("NewsPartial", userNews);
        }

        [HttpGet]
        public async Task<IActionResult> MyFavorites(int page = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favoriteNews = await _profileService.GetUserFavoritesAsync(userId, page, 9);
            
            ViewBag.CurrentPage = page;
            ViewBag.PageTitle = "My Favorites";
            
            return View("NewsPartial", favoriteNews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int newsId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isFavorite = await _favoriteService.IsFavoriteAsync(userId, newsId);

            bool success;
            if (isFavorite)
            {
                success = await _favoriteService.RemoveFromFavoritesAsync(userId, newsId);
                TempData["SuccessMessage"] = "Article removed from favorites!";
            }
            else
            {
                success = await _favoriteService.AddToFavoritesAsync(userId, newsId);
                TempData["SuccessMessage"] = "Article added to favorites!";
            }

            if (!success)
            {
                TempData["ErrorMessage"] = "Failed to update favorites. Please try again.";
            }

            return RedirectToAction(nameof(Index), new { tab = "favorites" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMyNews(int newsId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var canDelete = await _newsManagementService.CanUserDeleteNewsAsync(userId, newsId);

            if (!canDelete)
            {
                TempData["ErrorMessage"] = "You don't have permission to delete this article.";
                return RedirectToAction(nameof(Index), new { tab = "articles" });
            }

            // Use NewsController for actual deletion (DRY principle)
            return RedirectToAction("Delete", "News", new { id = newsId, returnUrl = Url.Action("Index", "Profile", new { tab = "articles" }) });
        }

        [HttpGet]
        public async Task<IActionResult> EditMyNews(int newsId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var canEdit = await _newsManagementService.CanUserEditNewsAsync(userId, newsId);

            if (!canEdit)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this article.";
                return RedirectToAction(nameof(Index), new { tab = "articles" });
            }

            // Use NewsController for actual editing (DRY principle)
            return RedirectToAction("Edit", "News", new { id = newsId, returnUrl = Url.Action("Index", "Profile", new { tab = "articles" }) });
        }

        [HttpGet]
        public async Task<IActionResult> Statistics()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var statistics = await _profileService.GetProfileStatisticsAsync(userId);
            
            return Json(statistics);
        }
    }
}
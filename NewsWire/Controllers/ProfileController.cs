using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewsWire.Models;
using NewsWire.Services.Interfaces;

namespace NewsWire.Controllers
{
    [Authorize]
    public class ProfileController : BaseController
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly SignInManager<CustomUser> _signInManager;
        private readonly IProfileService _profileService;
        private readonly IFavoriteService _favoriteService;
        private readonly INewsManagementService _newsManagementService;

        public ProfileController(
            UserManager<CustomUser> userManager,
            SignInManager<CustomUser> signInManager,
            IProfileService profileService,
            IFavoriteService favoriteService,
            INewsManagementService newsManagementService,
            ILogger<ProfileController> logger) : base(logger)
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
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return HandleNotFound("User", 0);

            var profile = await _profileService.GetProfileAsync(userId);
            if (profile == null)
                return HandleNotFound("Profile", 0);

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
        [RequestSizeLimit(10485760)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10485760)]
        public async Task<IActionResult> UpdateProfile([Bind(Prefix = "Profile")] ProfileViewModel model)
        {
            var currentUserId = GetCurrentUserId();

            if (!ModelState.IsValid)
            {
                var profileData = await _profileService.GetProfileAsync(currentUserId);
                var statistics = await _profileService.GetProfileStatisticsAsync(currentUserId);
                var userNews = await _profileService.GetUserNewsAsync(currentUserId, 1, 6);
                var favoriteNews = await _profileService.GetUserFavoritesAsync(currentUserId, 1, 6);

                var viewModel = new UserProfileDashboardViewModel
                {
                    Profile = model,
                    Statistics = statistics,
                    UserNews = userNews,
                    FavoriteNews = favoriteNews,
                    ActiveTab = "profile"
                };

                return View("Index", viewModel);
            }

            try
            {
                var currentUser = await _userManager.FindByIdAsync(currentUserId);
                if (currentUser == null)
                {
                    SetErrorMessage("User not found. Please log in again.");
                    return RedirectToAction(nameof(Index), new { tab = "profile" });
                }

                bool emailChanged = !string.IsNullOrEmpty(model.Email)
                    && !string.IsNullOrEmpty(currentUser.Email)
                    && !currentUser.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase);

                var success = await _profileService.UpdateProfileAsync(currentUserId, model);

                if (success)
                {
                    if (emailChanged)
                    {
                        var emailUpdateSuccess = await _profileService.UpdateEmailAsync(currentUserId, model.Email);
                        if (!emailUpdateSuccess)
                        {
                            SetErrorMessage("Profile updated but failed to update email. Email may already be in use.");
                            return RedirectToAction(nameof(Index), new { tab = "profile" });
                        }
                    }

                    var refreshUser = await _userManager.FindByIdAsync(currentUserId);
                    if (refreshUser != null)
                        await _signInManager.RefreshSignInAsync(refreshUser);

                    SetSuccessMessage("Profile updated successfully!");
                }
                else
                {
                    SetErrorMessage("Failed to update profile. Please try again.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user: {UserId}", currentUserId);
                SetErrorMessage("An error occurred while updating your profile.");
            }

            return RedirectToAction(nameof(Index), new { tab = "profile" });
        }

        [HttpGet]
        public async Task<IActionResult> MyNews(int page = 1)
        {
            var userId = GetCurrentUserId();
            var userNews = await _profileService.GetUserNewsAsync(userId, page, 6);

            ViewBag.CurrentPage = page;
            ViewBag.PageTitle = "My Articles";

            return View("NewsPartial", userNews);
        }

        [HttpGet]
        public async Task<IActionResult> MyFavorites(int page = 1)
        {
            var userId = GetCurrentUserId();
            var favoriteNews = await _profileService.GetUserFavoritesAsync(userId, page, 9);

            ViewBag.CurrentPage = page;
            ViewBag.PageTitle = "My Favorites";

            return View("NewsPartial", favoriteNews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int newsId)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Json(new { success = false, message = "Please login first." });

            var isFavorite = await _favoriteService.IsFavoriteAsync(userId, newsId);

            bool success;
            if (isFavorite)
                success = await _favoriteService.RemoveFromFavoritesAsync(userId, newsId);
            else
                success = await _favoriteService.AddToFavoritesAsync(userId, newsId);

            if (!success)
                return Json(new { success = false, message = "Failed to update favorites. Please try again." });

            return Json(new
            {
                success = true,
                isFavorite = !isFavorite,
                message = isFavorite ? "Removed from favorites!" : "Added to favorites!"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMyNews(int newsId)
        {
            var userId = GetCurrentUserId();
            var canDelete = await _newsManagementService.CanUserDeleteNewsAsync(userId, newsId);

            if (!canDelete)
            {
                SetErrorMessage("You don't have permission to delete this article.");
                return RedirectToAction(nameof(Index), new { tab = "articles" });
            }

            return RedirectToAction("Delete", "News", new { id = newsId, returnUrl = Url.Action("Index", "Profile", new { tab = "articles" }) });
        }

        [HttpGet]
        public async Task<IActionResult> EditMyNews(int newsId)
        {
            var userId = GetCurrentUserId();
            var canEdit = await _newsManagementService.CanUserEditNewsAsync(userId, newsId);

            if (!canEdit)
            {
                SetErrorMessage("You don't have permission to edit this article.");
                return RedirectToAction(nameof(Index), new { tab = "articles" });
            }

            return RedirectToAction("Edit", "News", new { id = newsId, returnUrl = Url.Action("Index", "Profile", new { tab = "articles" }) });
        }

        [HttpGet]
        public async Task<IActionResult> Statistics()
        {
            var userId = GetCurrentUserId();
            var statistics = await _profileService.GetProfileStatisticsAsync(userId);

            return Json(statistics);
        }
    }
}
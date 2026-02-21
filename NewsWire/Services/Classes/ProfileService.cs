using Microsoft.AspNetCore.Identity;
using NewsWire.Models;
using NewsWire.Repositories.Interfaces;
using NewsWire.Services.Interfaces;

namespace NewsWire.Services.Classes
{
    public class ProfileService : IProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<CustomUser> _userManager;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<ProfileService> _logger;

        public ProfileService(
            IUnitOfWork unitOfWork,
            UserManager<CustomUser> userManager,
            IFileUploadService fileUploadService,
            ILogger<ProfileService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        public async Task<ProfileViewModel?> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            var userNewsCount = await _unitOfWork.News.CountAsync(n => n.AuthorId == userId);
            var userFavoritesCount = await _unitOfWork.UserFavorites.CountAsync(f => f.UserId == userId);

            return new ProfileViewModel
            {
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                CurrentPicturePath = user.ProfilePictureUrl ?? "/assets/img/Local/default-avatar.jpg",
                TotalNewsCount = userNewsCount,
                TotalFavoriteCount = userFavoritesCount,
                JoinDate = user.LockoutEnd?.DateTime ?? DateTime.UtcNow
            };
        }

        public async Task<bool> UpdateProfileAsync(string userId, ProfileViewModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                if (model.ProfileImage != null && _fileUploadService.ValidateImageFile(model.ProfileImage))
                {
                    if (!string.IsNullOrEmpty(user.ProfilePictureUrl)
                        && !user.ProfilePictureUrl.Contains("default"))
                    {
                        await _fileUploadService.DeleteImageAsync(user.ProfilePictureUrl);
                    }

                    var imagePath = await _fileUploadService.UploadImageAsync(model.ProfileImage, "Profiles");
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        user.ProfilePictureUrl = imagePath;
                    }
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;

                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> UpdateEmailAsync(string userId, string newEmail)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                var existingUser = await _userManager.FindByEmailAsync(newEmail);
                if (existingUser != null && existingUser.Id != userId)
                    return false;

                var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
                var result = await _userManager.ChangeEmailAsync(user, newEmail, token);

                if (result.Succeeded)
                {
                    user.UserName = newEmail;
                    await _userManager.UpdateAsync(user);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating email for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<ProfileStatisticsViewModel> GetProfileStatisticsAsync(string userId)
        {
            var currentDate = DateTime.UtcNow;
            var startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);

            var userNews = (await _unitOfWork.News.GetNewsByAuthorAsync(userId)).ToList();

            var favorites = await _unitOfWork.UserFavorites.CountAsync(f => f.UserId == userId);

            var categoryStats = userNews.Any()
                ? userNews
                    .GroupBy(n => n.Category?.Name ?? "Unknown")
                    .Select(g => new CategoryStatistic
                    {
                        CategoryName = g.Key,
                        ArticleCount = g.Count(),
                        Percentage = Math.Round((double)g.Count() / userNews.Count * 100, 1)
                    })
                    .OrderByDescending(c => c.ArticleCount)
                    .ToList()
                : new List<CategoryStatistic>();

            var monthlyStats = userNews
                .Where(n => n.PublishedAt >= currentDate.AddMonths(-6))
                .GroupBy(n => new { n.PublishedAt.Year, n.PublishedAt.Month })
                .Select(g => new MonthlyActivity
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    ArticleCount = g.Count()
                })
                .OrderBy(m => m.Month)
                .ToList();

            return new ProfileStatisticsViewModel
            {
                TotalArticles = userNews.Count,
                PublishedThisMonth = userNews.Count(n => n.PublishedAt >= startOfMonth),
                TotalViews = 0,
                TotalFavorites = favorites,
                CategoryBreakdown = categoryStats,
                ActivityChart = monthlyStats
            };
        }

        public async Task<List<NewsDisplayViewModel>> GetUserNewsAsync(string userId, int page = 1, int pageSize = 6)
        {
            var userNews = await _unitOfWork.News.GetNewsByAuthorPagedAsync(userId, page, pageSize);

            return userNews.Select(news => new NewsDisplayViewModel
            {
                News = news,
                IsOwner = true,
                IsFavorite = false
            }).ToList();
        }

        public async Task<List<NewsDisplayViewModel>> GetUserFavoritesAsync(string userId, int page = 1, int pageSize = 6)
        {
            var favorites = await _unitOfWork.UserFavorites.GetUserFavoritesPagedAsync(userId, page, pageSize);

            return favorites.Select(favorite => new NewsDisplayViewModel
            {
                News = favorite.News,
                IsOwner = favorite.News.AuthorId == userId,
                IsFavorite = true
            }).ToList();
        }
    }
}

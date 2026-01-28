using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewsWire.Data;
using NewsWire.Models;

namespace NewsWire.Services
{
    public class ProfileService : IProfileService
    {
        private readonly NewsDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<ProfileService> _logger;

        public ProfileService(
            NewsDbContext context, 
            UserManager<CustomUser> userManager, 
            IFileUploadService fileUploadService,
            ILogger<ProfileService> logger)
        {
            _context = context;
            _userManager = userManager;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        public async Task<ProfileViewModel> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return null;
            }

            var userNewsCount = await _context.News.CountAsync(n => n.AuthorId == userId);
            var userFavoritesCount = await _context.UserFavorites.CountAsync(f => f.UserId == userId);

            return new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CurrentPicturePath = user.profilePictureUrl ?? "/assets/img/Local/default-avatar.jpg",
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
                {
                    _logger.LogWarning("User not found for update: {UserId}", userId);
                    return false;
                }

                if (model.ProfileImage != null && _fileUploadService.ValidateImageFile(model.ProfileImage))
                {
                    if (!string.IsNullOrEmpty(user.profilePictureUrl))
                    {
                        await _fileUploadService.DeleteImageAsync(user.profilePictureUrl);
                    }

                    var imagePath = await _fileUploadService.UploadImageAsync(model.ProfileImage, "Profiles");
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        user.profilePictureUrl = imagePath;
                        _logger.LogInformation("Profile image updated for user: {UserId}", userId);
                    }
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;

                var result = await _userManager.UpdateAsync(user);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("Profile updated successfully for user: {UserId}", userId);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to update profile: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    return false;
                }
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
                {
                    _logger.LogWarning("User not found for email update: {UserId}", userId);
                    return false;
                }

                var existingUser = await _userManager.FindByEmailAsync(newEmail);
                if (existingUser != null && existingUser.Id != userId)
                {
                    _logger.LogWarning("Email already in use: {Email}", newEmail);
                    return false;
                }

                var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
                var result = await _userManager.ChangeEmailAsync(user, newEmail, token);

                if (result.Succeeded)
                {
                    user.UserName = newEmail;
                    await _userManager.UpdateAsync(user);
                    _logger.LogInformation("Email updated successfully for user: {UserId}", userId);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to update email: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    return false;
                }
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

            var userNews = await _context.News
                .Where(n => n.AuthorId == userId)
                .Include(n => n.Category)
                .ToListAsync();

            var favorites = await _context.UserFavorites
                .Where(f => f.UserId == userId)
                .CountAsync();

            var categoryStats = userNews.Any()
                ? userNews
                    .GroupBy(n => n.Category.Name)
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
            var userNews = await _context.News
                .Where(n => n.AuthorId == userId)
                .Include(n => n.Category)
                .OrderByDescending(n => n.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return userNews.Select(news => new NewsDisplayViewModel
            {
                News = news,
                IsOwner = true,
                IsFavorite = false
            }).ToList();
        }

        public async Task<List<NewsDisplayViewModel>> GetUserFavoritesAsync(string userId, int page = 1, int pageSize = 6)
        {
            var favorites = await _context.UserFavorites
                .Where(f => f.UserId == userId)
                .Include(f => f.News)
                .ThenInclude(n => n.Category)
                .OrderByDescending(f => f.FavoritedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return favorites.Select(favorite => new NewsDisplayViewModel
            {
                News = favorite.News,
                IsOwner = favorite.News.AuthorId == userId,
                IsFavorite = true
            }).ToList();
        }
    }

    public class FavoriteService : IFavoriteService
    {
        private readonly NewsDbContext _context;

        public FavoriteService(NewsDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddToFavoritesAsync(string userId, int newsId)
        {
            var existingFavorite = await _context.UserFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.NewsId == newsId);

            if (existingFavorite != null) return false;

            var favorite = new UserFavorite
            {
                UserId = userId,
                NewsId = newsId,
                FavoritedAt = DateTime.UtcNow
            };

            _context.UserFavorites.Add(favorite);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveFromFavoritesAsync(string userId, int newsId)
        {
            var favorite = await _context.UserFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.NewsId == newsId);

            if (favorite == null) return false;

            _context.UserFavorites.Remove(favorite);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> IsFavoriteAsync(string userId, int newsId)
        {
            return await _context.UserFavorites
                .AnyAsync(f => f.UserId == userId && f.NewsId == newsId);
        }

        public async Task<List<UserFavorite>> GetUserFavoritesAsync(string userId)
        {
            return await _context.UserFavorites
                .Where(f => f.UserId == userId)
                .Include(f => f.News)
                .ThenInclude(n => n.Category)
                .ToListAsync();
        }
    }

    public class NewsManagementService : INewsManagementService
    {
        private readonly NewsDbContext _context;

        public NewsManagementService(NewsDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CanUserEditNewsAsync(string userId, int newsId)
        {
            var news = await _context.News.FindAsync(newsId);
            return news != null && news.AuthorId == userId;
        }

        public async Task<bool> CanUserDeleteNewsAsync(string userId, int newsId)
        {
            var news = await _context.News.FindAsync(newsId);
            return news != null && news.AuthorId == userId;
        }

        public async Task<News> GetUserNewsAsync(string userId, int newsId)
        {
            return await _context.News
                .Include(n => n.Category)
                .FirstOrDefaultAsync(n => n.Id == newsId && n.AuthorId == userId);
        }
    }
}
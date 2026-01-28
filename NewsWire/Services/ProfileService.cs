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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileService(NewsDbContext context, UserManager<CustomUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<ProfileViewModel> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var userNewsCount = await _context.News.CountAsync(n => n.AuthorId == userId);
            var userFavoritesCount = await _context.UserFavorites.CountAsync(f => f.UserId == userId);

            return new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CurrentPicturePath = user.profilePictureUrl,
                TotalNewsCount = userNewsCount,
                TotalFavoriteCount = userFavoritesCount,
                JoinDate = user.LockoutEnd?.DateTime ?? DateTime.UtcNow
            };
        }

        public async Task<bool> UpdateProfileAsync(string userId, ProfileViewModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // Handle profile image upload
            if (model.ProfileImage != null)
            {
                var imagePath = await SaveProfileImageAsync(model.ProfileImage);
                if (!string.IsNullOrEmpty(imagePath))
                {
                    user.profilePictureUrl = imagePath;
                }
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
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

            var categoryStats = userNews
                .GroupBy(n => n.Category.Name)
                .Select(g => new CategoryStatistic
                {
                    CategoryName = g.Key,
                    ArticleCount = g.Count(),
                    Percentage = Math.Round((double)g.Count() / userNews.Count * 100, 1)
                })
                .OrderByDescending(c => c.ArticleCount)
                .ToList();

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

        private async Task<string> SaveProfileImageAsync(IFormFile image)
        {
            try
            {
                string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profiles");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                string filePath = Path.Combine(folderPath, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                return "/images/profiles/" + uniqueFileName;
            }
            catch
            {
                return null;
            }
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
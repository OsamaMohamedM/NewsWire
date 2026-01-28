using Microsoft.EntityFrameworkCore;
using NewsWire.Data;
using NewsWire.Models;

namespace NewsWire.Services
{
    public class NewsService : INewsService
    {
        private readonly NewsDbContext _context;
        private readonly IFavoriteService _favoriteService;
        private readonly ILogger<NewsService> _logger;

        public NewsService(
            NewsDbContext context,
            IFavoriteService favoriteService,
            ILogger<NewsService> logger)
        {
            _context = context;
            _favoriteService = favoriteService;
            _logger = logger;
        }

        public async Task<List<NewsDisplayViewModel>> GetNewsByCategoryAsync(int categoryId, int page, int pageSize, string? userId)
        {
            try
            {
                var source = _context.News
                    .Where(n => n.CategoryId == categoryId)
                    .Include(n => n.Category)
                    .OrderByDescending(n => n.PublishedAt);

                var pagedNews = await source
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                HashSet<int> favoriteNewsIds = new();
                if (userId != null)
                {
                    var favorites = await _favoriteService.GetUserFavoritesAsync(userId);
                    favoriteNewsIds = favorites.Select(f => f.NewsId).ToHashSet();
                }

                return pagedNews.Select(newsItem => new NewsDisplayViewModel
                {
                    News = newsItem,
                    IsOwner = userId != null && newsItem.AuthorId == userId,
                    IsFavorite = favoriteNewsIds.Contains(newsItem.Id)
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving news for category: {CategoryId}", categoryId);
                return new List<NewsDisplayViewModel>();
            }
        }

        public async Task<NewsDisplayViewModel?> GetNewsDetailsAsync(int newsId, string? userId)
        {
            try
            {
                var news = await _context.News
                    .Include(n => n.Category)
                    .FirstOrDefaultAsync(m => m.Id == newsId);

                if (news == null)
                {
                    _logger.LogWarning("News not found: {NewsId}", newsId);
                    return null;
                }

                var isFavorite = userId != null && await _favoriteService.IsFavoriteAsync(userId, newsId);

                return new NewsDisplayViewModel
                {
                    News = news,
                    IsOwner = news.AuthorId == userId,
                    IsFavorite = isFavorite
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving news details: {NewsId}", newsId);
                return null;
            }
        }

        public async Task<News?> GetNewsByIdAsync(int id)
        {
            try
            {
                return await _context.News.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving news: {NewsId}", id);
                return null;
            }
        }

        public async Task<bool> CreateNewsAsync(News news)
        {
            try
            {
                if (news == null)
                {
                    _logger.LogWarning("Attempted to create null news");
                    return false;
                }

                _context.News.Add(news);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("News created: {NewsTitle} by {AuthorId}", news.Title, news.AuthorId);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating news: {NewsTitle}", news?.Title);
                return false;
            }
        }

        public async Task<bool> UpdateNewsAsync(News news, string? oldImageUrl)
        {
            try
            {
                if (news == null)
                {
                    _logger.LogWarning("Attempted to update null news");
                    return false;
                }

                var existingNews = await _context.News.FindAsync(news.Id);
                if (existingNews == null)
                {
                    _logger.LogWarning("News not found for update: {NewsId}", news.Id);
                    return false;
                }

                existingNews.Title = news.Title;
                existingNews.Content = news.Content;
                existingNews.Topic = news.Topic;
                existingNews.CategoryId = news.CategoryId;
                existingNews.ImageUrl = news.ImageUrl;

                _context.Entry(existingNews).State = EntityState.Modified;
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("News updated: {NewsId}", news.Id);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating news: {NewsId}", news?.Id);
                return false;
            }
        }

        public async Task<bool> DeleteNewsAsync(int id)
        {
            try
            {
                var news = await _context.News.FindAsync(id);
                if (news == null)
                {
                    _logger.LogWarning("News not found for deletion: {NewsId}", id);
                    return false;
                }

                _context.News.Remove(news);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("News deleted: {NewsId}", id);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting news: {NewsId}", id);
                return false;
            }
        }

        public async Task<bool> UserCanEditNewsAsync(string userId, int newsId)
        {
            try
            {
                var news = await _context.News.FindAsync(newsId);
                return news != null && news.AuthorId == userId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking edit permission: {NewsId}", newsId);
                return false;
            }
        }

        public async Task<bool> UserCanDeleteNewsAsync(string userId, int newsId)
        {
            try
            {
                var news = await _context.News.FindAsync(newsId);
                return news != null && news.AuthorId == userId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking delete permission: {NewsId}", newsId);
                return false;
            }
        }

        public async Task<(int totalItems, int totalPages)> GetPaginationInfoAsync(int categoryId, int pageSize)
        {
            try
            {
                var totalItems = await _context.News
                    .Where(n => n.CategoryId == categoryId)
                    .CountAsync();

                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                return (totalItems, totalPages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating pagination: {CategoryId}", categoryId);
                return (0, 0);
            }
        }
    }
}
using NewsWire.Models;
using NewsWire.Repositories.Interfaces;
using NewsWire.Services.Interfaces;

namespace NewsWire.Services.Classes
{
    public class NewsService : INewsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFavoriteService _favoriteService;
        private readonly ILogger<NewsService> _logger;

        public NewsService(
            IUnitOfWork unitOfWork,
            IFavoriteService favoriteService,
            ILogger<NewsService> logger)
        {
            _unitOfWork = unitOfWork;
            _favoriteService = favoriteService;
            _logger = logger;
        }

        public async Task<List<NewsDisplayViewModel>> GetNewsByCategoryAsync(int categoryId, int page, int pageSize, string? userId)
        {
            try
            {
                var pagedNews = await _unitOfWork.News.GetNewsByCategoryPagedAsync(categoryId, page, pageSize);

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
                var news = await _unitOfWork.News.GetNewsWithCategoryAsync(newsId);
                if (news == null)
                    return null;

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
                return await _unitOfWork.News.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving news: {NewsId}", id);
                return null;
            }
        }

        public async Task<List<News>> GetAllNewsWithDetailsAsync()
        {
            try
            {
                var news = await _unitOfWork.News.GetAllWithDetailsAsync();
                return news.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all news with details");
                return new List<News>();
            }
        }

        public async Task<News?> GetNewsWithDetailsAsync(int id)
        {
            try
            {
                return await _unitOfWork.News.GetNewsWithDetailsAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving news with details: {NewsId}", id);
                return null;
            }
        }

        public async Task<bool> CreateNewsAsync(News news)
        {
            try
            {
                if (news == null)
                    return false;

                await _unitOfWork.News.AddAsync(news);
                var result = await _unitOfWork.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating news: {NewsTitle}", news?.Title);
                return false;
            }
        }

        public async Task<bool> UpdateNewsAsync(News news)
        {
            try
            {
                if (news == null)
                    return false;

                var existingNews = await _unitOfWork.News.GetByIdAsync(news.Id);
                if (existingNews == null)
                    return false;

                existingNews.Title = news.Title;
                existingNews.Content = news.Content;
                existingNews.Topic = news.Topic;
                existingNews.CategoryId = news.CategoryId;
                existingNews.ImageUrl = news.ImageUrl;
                existingNews.AuthorId = news.AuthorId;
                existingNews.PublishedAt = news.PublishedAt;

                _unitOfWork.News.Update(existingNews);
                var result = await _unitOfWork.SaveChangesAsync();
                return result > 0;
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
                var news = await _unitOfWork.News.GetByIdAsync(id);
                if (news == null)
                    return false;

                _unitOfWork.News.Remove(news);
                var result = await _unitOfWork.SaveChangesAsync();
                return result > 0;
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
                var news = await _unitOfWork.News.GetByIdAsync(newsId);
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
                var news = await _unitOfWork.News.GetByIdAsync(newsId);
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
                var totalItems = await _unitOfWork.News.CountAsync(n => n.CategoryId == categoryId);
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                return (totalItems, totalPages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating pagination: {CategoryId}", categoryId);
                return (0, 0);
            }
        }

        public async Task<bool> NewsExistsAsync(int id)
        {
            try
            {
                return await _unitOfWork.News.ExistsAsync(n => n.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking news existence: {NewsId}", id);
                return false;
            }
        }
    }
}

using NewsWire.Models;

namespace NewsWire.Services.Interfaces
{
    public interface INewsService
    {
        Task<List<NewsDisplayViewModel>> GetNewsByCategoryAsync(int categoryId, int page, int pageSize, string? userId);
        Task<NewsDisplayViewModel?> GetNewsDetailsAsync(int newsId, string? userId);
        Task<News?> GetNewsByIdAsync(int id);
        Task<List<News>> GetAllNewsWithDetailsAsync();
        Task<News?> GetNewsWithDetailsAsync(int id);
        Task<bool> CreateNewsAsync(News news);
        Task<bool> UpdateNewsAsync(News news);
        Task<bool> DeleteNewsAsync(int id);
        Task<bool> UserCanEditNewsAsync(string userId, int newsId);
        Task<bool> UserCanDeleteNewsAsync(string userId, int newsId);
        Task<(int totalItems, int totalPages)> GetPaginationInfoAsync(int categoryId, int pageSize);
        Task<bool> NewsExistsAsync(int id);
    }
}

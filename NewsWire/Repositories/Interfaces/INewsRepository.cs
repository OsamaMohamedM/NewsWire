using NewsWire.Models;

namespace NewsWire.Repositories.Interfaces
{
    public interface INewsRepository : IRepository<News>
    {
        Task<IEnumerable<News>> GetNewsByCategoryPagedAsync(int categoryId, int page, int pageSize);
        Task<News?> GetNewsWithCategoryAsync(int id);
        Task<News?> GetNewsWithDetailsAsync(int id);
        Task<IEnumerable<News>> GetNewsByAuthorPagedAsync(string authorId, int page, int pageSize);
        Task<IEnumerable<News>> GetAllWithDetailsAsync();
        Task<IEnumerable<News>> GetNewsByAuthorAsync(string authorId);
    }
}

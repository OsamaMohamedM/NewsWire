using NewsWire.Models;

namespace NewsWire.Repositories.Interfaces
{
    public interface IUserFavoriteRepository : IRepository<UserFavorite>
    {
        Task<UserFavorite?> GetFavoriteAsync(string userId, int newsId);
        Task<IEnumerable<UserFavorite>> GetUserFavoritesWithDetailsAsync(string userId);
        Task<IEnumerable<UserFavorite>> GetUserFavoritesPagedAsync(string userId, int page, int pageSize);
    }
}

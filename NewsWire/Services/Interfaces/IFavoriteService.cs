using NewsWire.Models;

namespace NewsWire.Services.Interfaces
{
    public interface IFavoriteService
    {
        Task<bool> AddToFavoritesAsync(string userId, int newsId);
        Task<bool> RemoveFromFavoritesAsync(string userId, int newsId);
        Task<bool> IsFavoriteAsync(string userId, int newsId);
        Task<List<UserFavorite>> GetUserFavoritesAsync(string userId);
    }
}

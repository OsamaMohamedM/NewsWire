using NewsWire.Models;
using NewsWire.Repositories.Interfaces;
using NewsWire.Services.Interfaces;

namespace NewsWire.Services.Classes
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FavoriteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddToFavoritesAsync(string userId, int newsId)
        {
            var existingFavorite = await _unitOfWork.UserFavorites.GetFavoriteAsync(userId, newsId);
            if (existingFavorite != null)
                return false;

            var favorite = new UserFavorite
            {
                UserId = userId,
                NewsId = newsId,
                FavoritedAt = DateTime.UtcNow
            };

            await _unitOfWork.UserFavorites.AddAsync(favorite);
            return await _unitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveFromFavoritesAsync(string userId, int newsId)
        {
            var favorite = await _unitOfWork.UserFavorites.GetFavoriteAsync(userId, newsId);
            if (favorite == null)
                return false;

            _unitOfWork.UserFavorites.Remove(favorite);
            return await _unitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<bool> IsFavoriteAsync(string userId, int newsId)
        {
            return await _unitOfWork.UserFavorites
                .ExistsAsync(f => f.UserId == userId && f.NewsId == newsId);
        }

        public async Task<List<UserFavorite>> GetUserFavoritesAsync(string userId)
        {
            var favorites = await _unitOfWork.UserFavorites.GetUserFavoritesWithDetailsAsync(userId);
            return favorites.ToList();
        }
    }
}

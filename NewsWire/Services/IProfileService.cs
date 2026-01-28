using NewsWire.Models;

namespace NewsWire.Services
{
    public interface IProfileService
    {
        Task<ProfileViewModel> GetProfileAsync(string userId);

        Task<bool> UpdateProfileAsync(string userId, ProfileViewModel model);

        Task<bool> UpdateEmailAsync(string userId, string newEmail);

        Task<ProfileStatisticsViewModel> GetProfileStatisticsAsync(string userId);

        Task<List<NewsDisplayViewModel>> GetUserNewsAsync(string userId, int page = 1, int pageSize = 6);

        Task<List<NewsDisplayViewModel>> GetUserFavoritesAsync(string userId, int page = 1, int pageSize = 6);
    }

    public interface IFavoriteService
    {
        Task<bool> AddToFavoritesAsync(string userId, int newsId);

        Task<bool> RemoveFromFavoritesAsync(string userId, int newsId);

        Task<bool> IsFavoriteAsync(string userId, int newsId);

        Task<List<UserFavorite>> GetUserFavoritesAsync(string userId);
    }

    public interface INewsManagementService
    {
        Task<bool> CanUserEditNewsAsync(string userId, int newsId);

        Task<bool> CanUserDeleteNewsAsync(string userId, int newsId);

        Task<News> GetUserNewsAsync(string userId, int newsId);
    }
}
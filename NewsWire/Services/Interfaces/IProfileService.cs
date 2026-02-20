using NewsWire.Models;

namespace NewsWire.Services.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileViewModel?> GetProfileAsync(string userId);
        Task<bool> UpdateProfileAsync(string userId, ProfileViewModel model);
        Task<bool> UpdateEmailAsync(string userId, string newEmail);
        Task<ProfileStatisticsViewModel> GetProfileStatisticsAsync(string userId);
        Task<List<NewsDisplayViewModel>> GetUserNewsAsync(string userId, int page = 1, int pageSize = 6);
        Task<List<NewsDisplayViewModel>> GetUserFavoritesAsync(string userId, int page = 1, int pageSize = 6);
    }
}

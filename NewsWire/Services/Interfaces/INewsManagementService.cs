using NewsWire.Models;

namespace NewsWire.Services.Interfaces
{
    public interface INewsManagementService
    {
        Task<bool> CanUserEditNewsAsync(string userId, int newsId);
        Task<bool> CanUserDeleteNewsAsync(string userId, int newsId);
        Task<News?> GetUserNewsAsync(string userId, int newsId);
    }
}

using NewsWire.Models;
using NewsWire.Repositories.Interfaces;
using NewsWire.Services.Interfaces;

namespace NewsWire.Services.Classes
{
    public class NewsManagementService : INewsManagementService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NewsManagementService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CanUserEditNewsAsync(string userId, int newsId)
        {
            var news = await _unitOfWork.News.GetByIdAsync(newsId);
            return news != null && news.AuthorId == userId;
        }

        public async Task<bool> CanUserDeleteNewsAsync(string userId, int newsId)
        {
            var news = await _unitOfWork.News.GetByIdAsync(newsId);
            return news != null && news.AuthorId == userId;
        }

        public async Task<News?> GetUserNewsAsync(string userId, int newsId)
        {
            return await _unitOfWork.News.GetNewsWithCategoryAsync(newsId) is News news && news.AuthorId == userId
                ? news
                : null;
        }
    }
}

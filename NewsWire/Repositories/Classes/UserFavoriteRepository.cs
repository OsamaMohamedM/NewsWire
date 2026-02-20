using Microsoft.EntityFrameworkCore;
using NewsWire.Data;
using NewsWire.Models;
using NewsWire.Repositories.Interfaces;

namespace NewsWire.Repositories.Classes
{
    public class UserFavoriteRepository : Repository<UserFavorite>, IUserFavoriteRepository
    {
        public UserFavoriteRepository(NewsDbContext context) : base(context)
        {
        }

        public async Task<UserFavorite?> GetFavoriteAsync(string userId, int newsId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(f => f.UserId == userId && f.NewsId == newsId);
        }

        public async Task<IEnumerable<UserFavorite>> GetUserFavoritesWithDetailsAsync(string userId)
        {
            return await _dbSet
                .Where(f => f.UserId == userId)
                .Include(f => f.News)
                .ThenInclude(n => n.Category)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserFavorite>> GetUserFavoritesPagedAsync(string userId, int page, int pageSize)
        {
            return await _dbSet
                .Where(f => f.UserId == userId)
                .Include(f => f.News)
                .ThenInclude(n => n.Category)
                .OrderByDescending(f => f.FavoritedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}

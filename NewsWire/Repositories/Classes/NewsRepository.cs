using Microsoft.EntityFrameworkCore;
using NewsWire.Data;
using NewsWire.Models;
using NewsWire.Repositories.Interfaces;

namespace NewsWire.Repositories.Classes
{
    public class NewsRepository : Repository<News>, INewsRepository
    {
        public NewsRepository(NewsDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<News>> GetNewsByCategoryPagedAsync(int categoryId, int page, int pageSize)
        {
            return await _dbSet
                .Where(n => n.CategoryId == categoryId)
                .Include(n => n.Category)
                .OrderByDescending(n => n.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<News?> GetNewsWithCategoryAsync(int id)
        {
            return await _dbSet
                .Include(n => n.Category)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<News?> GetNewsWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(n => n.Category)
                .Include(n => n.Author)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<IEnumerable<News>> GetNewsByAuthorPagedAsync(string authorId, int page, int pageSize)
        {
            return await _dbSet
                .Where(n => n.AuthorId == authorId)
                .Include(n => n.Category)
                .OrderByDescending(n => n.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<News>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(n => n.Category)
                .Include(n => n.Author)
                .OrderByDescending(n => n.PublishedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<News>> GetNewsByAuthorAsync(string authorId)
        {
            return await _dbSet
                .Where(n => n.AuthorId == authorId)
                .Include(n => n.Category)
                .ToListAsync();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using NewsWire.Data;
using NewsWire.Models;
using NewsWire.Repositories.Interfaces;

namespace NewsWire.Repositories.Classes
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(NewsDbContext context) : base(context)
        {
        }

        public async Task<Category?> GetCategoryWithNewsAsync(int id)
        {
            return await _dbSet
                .Include(c => c.NewsItems)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Category>> GetAllOrderedAsync()
        {
            return await _dbSet
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return await _dbSet.AnyAsync(c =>
                    c.Name.ToLower() == name.ToLower() && c.Id != excludeId.Value);
            }

            return await _dbSet.AnyAsync(c => c.Name.ToLower() == name.ToLower());
        }
    }
}

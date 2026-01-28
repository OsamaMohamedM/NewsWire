using Microsoft.EntityFrameworkCore;
using NewsWire.Data;
using NewsWire.Models;

namespace NewsWire.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly NewsDbContext _context;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(NewsDbContext context, ILogger<CategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            try
            {
                return await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all categories");
                return new List<Category>();
            }
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            try
            {
                return await _context.Categories
                    .Include(c => c.NewsItems)
                    .FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category with ID: {CategoryId}", id);
                return null;
            }
        }

        public async Task<bool> CreateCategoryAsync(Category category)
        {
            try
            {
                if (category == null)
                {
                    _logger.LogWarning("Attempted to create null category");
                    return false;
                }

                // Check for duplicate category name
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == category.Name.ToLower());

                if (existingCategory != null)
                {
                    _logger.LogWarning("Category with name '{CategoryName}' already exists", category.Name);
                    return false;
                }

                _context.Categories.Add(category);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Category created successfully: {CategoryName}", category.Name);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category: {CategoryName}", category?.Name);
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            try
            {
                if (category == null)
                {
                    _logger.LogWarning("Attempted to update null category");
                    return false;
                }

                var existingCategory = await _context.Categories.FindAsync(category.Id);
                if (existingCategory == null)
                {
                    _logger.LogWarning("Category not found for update: {CategoryId}", category.Id);
                    return false;
                }

                // Check for duplicate name (excluding current category)
                var duplicateCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == category.Name.ToLower() && c.Id != category.Id);

                if (duplicateCategory != null)
                {
                    _logger.LogWarning("Category name '{CategoryName}' already exists", category.Name);
                    return false;
                }

                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;

                _context.Entry(existingCategory).State = EntityState.Modified;
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Category updated successfully: {CategoryId}", category.Id);
                    return true;
                }

                return false;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating category: {CategoryId}", category?.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {CategoryId}", category?.Id);
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.NewsItems)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    _logger.LogWarning("Category not found for deletion: {CategoryId}", id);
                    return false;
                }

                // Check if category has news articles
                if (category.NewsItems != null && category.NewsItems.Any())
                {
                    _logger.LogWarning("Cannot delete category with existing news articles: {CategoryId}", id);
                    return false;
                }

                _context.Categories.Remove(category);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Category deleted successfully: {CategoryId}", id);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                return false;
            }
        }

        public async Task<bool> CategoryExistsAsync(int id)
        {
            try
            {
                return await _context.Categories.AnyAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking category existence: {CategoryId}", id);
                return false;
            }
        }

        public async Task<int> GetNewsCountByCategoryAsync(int categoryId)
        {
            try
            {
                return await _context.News
                    .Where(n => n.CategoryId == categoryId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting news for category: {CategoryId}", categoryId);
                return 0;
            }
        }
    }
}
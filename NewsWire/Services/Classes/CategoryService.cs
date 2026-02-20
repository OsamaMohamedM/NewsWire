using NewsWire.Models;
using NewsWire.Repositories.Interfaces;
using NewsWire.Services.Interfaces;

namespace NewsWire.Services.Classes
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(IUnitOfWork unitOfWork, ILogger<CategoryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetAllOrderedAsync();
                return categories.ToList();
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
                return await _unitOfWork.Categories.GetCategoryWithNewsAsync(id);
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
                    return false;

                if (await _unitOfWork.Categories.NameExistsAsync(category.Name))
                {
                    _logger.LogWarning("Category with name '{CategoryName}' already exists", category.Name);
                    return false;
                }

                await _unitOfWork.Categories.AddAsync(category);
                var result = await _unitOfWork.SaveChangesAsync();
                return result > 0;
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
                    return false;

                var existingCategory = await _unitOfWork.Categories.GetByIdAsync(category.Id);
                if (existingCategory == null)
                    return false;

                if (await _unitOfWork.Categories.NameExistsAsync(category.Name, category.Id))
                {
                    _logger.LogWarning("Category name '{CategoryName}' already exists", category.Name);
                    return false;
                }

                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;

                _unitOfWork.Categories.Update(existingCategory);
                var result = await _unitOfWork.SaveChangesAsync();
                return result > 0;
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
                var category = await _unitOfWork.Categories.GetCategoryWithNewsAsync(id);
                if (category == null)
                    return false;

                if (category.NewsItems != null && category.NewsItems.Any())
                {
                    _logger.LogWarning("Cannot delete category with existing news articles: {CategoryId}", id);
                    return false;
                }

                _unitOfWork.Categories.Remove(category);
                var result = await _unitOfWork.SaveChangesAsync();
                return result > 0;
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
                return await _unitOfWork.Categories.ExistsAsync(c => c.Id == id);
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
                return await _unitOfWork.News.CountAsync(n => n.CategoryId == categoryId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting news for category: {CategoryId}", categoryId);
                return 0;
            }
        }
    }
}

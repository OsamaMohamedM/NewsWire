using NewsWire.Models;

namespace NewsWire.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetCategoryWithNewsAsync(int id);
        Task<IEnumerable<Category>> GetAllOrderedAsync();
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
    }
}

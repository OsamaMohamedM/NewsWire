using NewsWire.Models;

namespace NewsWire.Repositories.Interfaces
{
    public interface IContactRepository : IRepository<ContactUs>
    {
        Task<IEnumerable<ContactUs>> GetAllOrderedAsync();
    }
}

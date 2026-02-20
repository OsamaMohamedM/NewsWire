using Microsoft.EntityFrameworkCore;
using NewsWire.Data;
using NewsWire.Models;
using NewsWire.Repositories.Interfaces;

namespace NewsWire.Repositories.Classes
{
    public class ContactRepository : Repository<ContactUs>, IContactRepository
    {
        public ContactRepository(NewsDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ContactUs>> GetAllOrderedAsync()
        {
            return await _dbSet
                .OrderByDescending(c => c.Id)
                .ToListAsync();
        }
    }
}

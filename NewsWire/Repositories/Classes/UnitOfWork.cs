using NewsWire.Data;
using NewsWire.Repositories.Interfaces;

namespace NewsWire.Repositories.Classes
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly NewsDbContext _context;

        public INewsRepository News { get; }
        public ICategoryRepository Categories { get; }
        public IContactRepository Contacts { get; }
        public ITeamMemberRepository TeamMembers { get; }
        public IUserFavoriteRepository UserFavorites { get; }

        public UnitOfWork(
            NewsDbContext context,
            INewsRepository newsRepository,
            ICategoryRepository categoryRepository,
            IContactRepository contactRepository,
            ITeamMemberRepository teamMemberRepository,
            IUserFavoriteRepository userFavoriteRepository)
        {
            _context = context;
            News = newsRepository;
            Categories = categoryRepository;
            Contacts = contactRepository;
            TeamMembers = teamMemberRepository;
            UserFavorites = userFavoriteRepository;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
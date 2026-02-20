namespace NewsWire.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        INewsRepository News { get; }
        ICategoryRepository Categories { get; }
        IContactRepository Contacts { get; }
        ITeamMemberRepository TeamMembers { get; }
        IUserFavoriteRepository UserFavorites { get; }
        Task<int> SaveChangesAsync();
    }
}

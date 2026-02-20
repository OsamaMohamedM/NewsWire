using NewsWire.Models;

namespace NewsWire.Services.Interfaces
{
    public interface ITeamMemberService
    {
        Task<List<TeamMember>> GetAllAsync();
        Task<TeamMember?> GetByIdAsync(int id);
        Task<bool> CreateAsync(TeamMember teamMember);
        Task<bool> UpdateAsync(TeamMember teamMember);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}

using NewsWire.Models;
using NewsWire.Repositories.Interfaces;
using NewsWire.Services.Interfaces;

namespace NewsWire.Services.Classes
{
    public class TeamMemberService : ITeamMemberService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TeamMemberService> _logger;

        public TeamMemberService(IUnitOfWork unitOfWork, ILogger<TeamMemberService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<TeamMember>> GetAllAsync()
        {
            try
            {
                var members = await _unitOfWork.TeamMembers.GetAllAsync();
                return members.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving team members");
                return new List<TeamMember>();
            }
        }

        public async Task<TeamMember?> GetByIdAsync(int id)
        {
            try
            {
                return await _unitOfWork.TeamMembers.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving team member: {Id}", id);
                return null;
            }
        }

        public async Task<bool> CreateAsync(TeamMember teamMember)
        {
            try
            {
                if (teamMember == null)
                    return false;

                await _unitOfWork.TeamMembers.AddAsync(teamMember);
                return await _unitOfWork.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating team member");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(TeamMember teamMember)
        {
            try
            {
                if (teamMember == null)
                    return false;

                var existing = await _unitOfWork.TeamMembers.GetByIdAsync(teamMember.Id);
                if (existing == null)
                    return false;

                existing.Name = teamMember.Name;
                existing.JobTitle = teamMember.JobTitle;
                existing.ImageUrl = teamMember.ImageUrl;

                _unitOfWork.TeamMembers.Update(existing);
                return await _unitOfWork.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating team member: {Id}", teamMember?.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var member = await _unitOfWork.TeamMembers.GetByIdAsync(id);
                if (member == null)
                    return false;

                _unitOfWork.TeamMembers.Remove(member);
                return await _unitOfWork.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting team member: {Id}", id);
                return false;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _unitOfWork.TeamMembers.ExistsAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking team member existence: {Id}", id);
                return false;
            }
        }
    }
}

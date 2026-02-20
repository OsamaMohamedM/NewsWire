using NewsWire.Data;
using NewsWire.Models;
using NewsWire.Repositories.Interfaces;

namespace NewsWire.Repositories.Classes
{
    public class TeamMemberRepository : Repository<TeamMember>, ITeamMemberRepository
    {
        public TeamMemberRepository(NewsDbContext context) : base(context)
        {
        }
    }
}

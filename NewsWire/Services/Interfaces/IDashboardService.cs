using NewsWire.Models;

namespace NewsWire.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardStats> GetDashboardStatsAsync();
    }
}

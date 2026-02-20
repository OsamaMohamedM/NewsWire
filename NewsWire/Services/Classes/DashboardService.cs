using Microsoft.AspNetCore.Identity;
using NewsWire.Models;
using NewsWire.Repositories.Interfaces;
using NewsWire.Services.Interfaces;

namespace NewsWire.Services.Classes
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<CustomUser> _userManager;

        public DashboardService(IUnitOfWork unitOfWork, UserManager<CustomUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            return new DashboardStats
            {
                TotalNews = await _unitOfWork.News.CountAsync(),
                TotalCategories = await _unitOfWork.Categories.CountAsync(),
                TotalTeamMembers = await _unitOfWork.TeamMembers.CountAsync(),
                TotalContactMessages = await _unitOfWork.Contacts.CountAsync(),
                TotalUsers = _userManager.Users.Count()
            };
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsWire.Data;
using NewsWire.Models;

namespace NewsWire.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly NewsDbContext _context;
        private readonly UserManager<CustomUser> _userManager;

        public DashboardController(NewsDbContext context, UserManager<CustomUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["PageTitle"] = "Dashboard Overview";

            var stats = new DashboardStats
            {
                TotalNews = await _context.News.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalTeamMembers = await _context.TeamMembers.CountAsync(),
                TotalContactMessages = await _context.ContactUs.CountAsync(),
                TotalUsers = _userManager.Users.Count()
            };

            return View(stats);
        }
    }

    public class DashboardStats
    {
        public int TotalNews { get; set; }
        public int TotalCategories { get; set; }
        public int TotalTeamMembers { get; set; }
        public int TotalContactMessages { get; set; }
        public int TotalUsers { get; set; }
    }
}

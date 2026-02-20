using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsWire.Models;
using NewsWire.Services.Interfaces;

namespace NewsWire.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TeamMembersController : Controller
    {
        private readonly ITeamMemberService _teamMemberService;

        public TeamMembersController(ITeamMemberService teamMemberService)
        {
            _teamMemberService = teamMemberService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["PageTitle"] = "Team Members Management";
            var teamMembers = await _teamMemberService.GetAllAsync();
            return View(teamMembers);
        }

        public async Task<IActionResult> Details(int? id)
        {
            ViewData["PageTitle"] = "Team Member Details";
            if (id == null)
                return NotFound();

            var teamMember = await _teamMemberService.GetByIdAsync(id.Value);
            if (teamMember == null)
                return NotFound();

            return View(teamMember);
        }

        public IActionResult Create()
        {
            ViewData["PageTitle"] = "Create Team Member";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,JobTitle,ImageUrl")] TeamMember teamMember)
        {
            if (ModelState.IsValid)
            {
                var success = await _teamMemberService.CreateAsync(teamMember);
                if (success)
                {
                    TempData["SuccessMessage"] = "Team member added successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(teamMember);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            ViewData["PageTitle"] = "Edit Team Member";
            if (id == null)
                return NotFound();

            var teamMember = await _teamMemberService.GetByIdAsync(id.Value);
            if (teamMember == null)
                return NotFound();

            return View(teamMember);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,JobTitle,ImageUrl")] TeamMember teamMember)
        {
            if (id != teamMember.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var success = await _teamMemberService.UpdateAsync(teamMember);
                if (success)
                {
                    TempData["SuccessMessage"] = "Team member updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                if (!await _teamMemberService.ExistsAsync(teamMember.Id))
                    return NotFound();
            }
            return View(teamMember);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            ViewData["PageTitle"] = "Delete Team Member";
            if (id == null)
                return NotFound();

            var teamMember = await _teamMemberService.GetByIdAsync(id.Value);
            if (teamMember == null)
                return NotFound();

            return View(teamMember);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _teamMemberService.DeleteAsync(id);
            if (success)
                TempData["SuccessMessage"] = "Team member deleted successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}

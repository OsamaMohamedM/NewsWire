using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsWire.Data;
using NewsWire.Models;

namespace NewsWire.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TeamMembersController : Controller
    {
        private readonly NewsDbContext _context;

        public TeamMembersController(NewsDbContext context)
        {
            _context = context;
        }

        // GET: Admin/TeamMembers
        public async Task<IActionResult> Index()
        {
            ViewData["PageTitle"] = "Team Members Management";
            var teamMembers = await _context.TeamMembers.ToListAsync();
            return View(teamMembers);
        }

        // GET: Admin/TeamMembers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewData["PageTitle"] = "Team Member Details";
            if (id == null)
            {
                return NotFound();
            }

            var teamMember = await _context.TeamMembers
                .FirstOrDefaultAsync(m => m.Id == id);

            if (teamMember == null)
            {
                return NotFound();
            }

            return View(teamMember);
        }

        // GET: Admin/TeamMembers/Create
        public IActionResult Create()
        {
            ViewData["PageTitle"] = "Create Team Member";
            return View();
        }

        // POST: Admin/TeamMembers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,JobTitle,ImageUrl")] TeamMember teamMember)
        {
            if (ModelState.IsValid)
            {
                _context.Add(teamMember);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Team member added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(teamMember);
        }

        // GET: Admin/TeamMembers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            ViewData["PageTitle"] = "Edit Team Member";
            if (id == null)
            {
                return NotFound();
            }

            var teamMember = await _context.TeamMembers.FindAsync(id);
            if (teamMember == null)
            {
                return NotFound();
            }
            return View(teamMember);
        }

        // POST: Admin/TeamMembers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,JobTitle,ImageUrl")] TeamMember teamMember)
        {
            if (id != teamMember.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teamMember);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Team member updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamMemberExists(teamMember.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(teamMember);
        }

        // GET: Admin/TeamMembers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            ViewData["PageTitle"] = "Delete Team Member";
            if (id == null)
            {
                return NotFound();
            }

            var teamMember = await _context.TeamMembers
                .FirstOrDefaultAsync(m => m.Id == id);

            if (teamMember == null)
            {
                return NotFound();
            }

            return View(teamMember);
        }

        // POST: Admin/TeamMembers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teamMember = await _context.TeamMembers.FindAsync(id);
            if (teamMember != null)
            {
                _context.TeamMembers.Remove(teamMember);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Team member deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TeamMemberExists(int id)
        {
            return _context.TeamMembers.Any(e => e.Id == id);
        }
    }
}

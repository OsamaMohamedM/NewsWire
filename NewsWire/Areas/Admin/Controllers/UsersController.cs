using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsWire.Models;

namespace NewsWire.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<CustomUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index()
        {
            ViewData["PageTitle"] = "Users & Roles Management";
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList()
                });
            }

            return View(userViewModels);
        }

        // GET: Admin/Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            ViewData["PageTitle"] = "User Details";
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var viewModel = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Roles = roles.ToList()
            };

            return View(viewModel);
        }

        // GET: Admin/Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            ViewData["PageTitle"] = "Edit User";
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();

            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                AllRoles = allRoles.Select(r => r.Name).ToList(),
                UserRoles = userRoles.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var selectedRoles = model.SelectedRoles ?? new List<string>();

                    var rolesToAdd = selectedRoles.Except(currentRoles);
                    var rolesToRemove = currentRoles.Except(selectedRoles);

                    if (rolesToAdd.Any())
                    {
                        var addRolesResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                        if (!addRolesResult.Succeeded)
                        {
                            _logger.LogWarning("Failed to add roles {Roles} to user {UserId}: {Errors}",
                                string.Join(", ", rolesToAdd), user.Id,
                                string.Join(", ", addRolesResult.Errors.Select(e => e.Description)));
                            TempData["ErrorMessage"] = string.Join(", ", addRolesResult.Errors.Select(e => e.Description));
                            var allRoles = await _roleManager.Roles.ToListAsync();
                            model.AllRoles = allRoles.Select(r => r.Name).ToList();
                            model.UserRoles = model.SelectedRoles ?? new List<string>();
                            return View(model);
                        }
                        _logger.LogInformation("Admin {AdminId} added roles {Roles} to user {UserId}",
                            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                            string.Join(", ", rolesToAdd), user.Id);
                    }

                    if (rolesToRemove.Any())
                    {
                        var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                        if (!removeRolesResult.Succeeded)
                        {
                            _logger.LogWarning("Failed to remove roles {Roles} from user {UserId}: {Errors}",
                                string.Join(", ", rolesToRemove), user.Id,
                                string.Join(", ", removeRolesResult.Errors.Select(e => e.Description)));
                            TempData["ErrorMessage"] = string.Join(", ", removeRolesResult.Errors.Select(e => e.Description));
                            var allRoles = await _roleManager.Roles.ToListAsync();
                            model.AllRoles = allRoles.Select(r => r.Name).ToList();
                            model.UserRoles = model.SelectedRoles ?? new List<string>();
                            return View(model);
                        }
                        _logger.LogInformation("Admin {AdminId} removed roles {Roles} from user {UserId}",
                            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                            string.Join(", ", rolesToRemove), user.Id);
                    }

                    TempData["SuccessMessage"] = "User updated successfully!";
                    _logger.LogInformation("User {UserId} profile updated successfully by admin {AdminId}",
                        user.Id, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            var rolesForView = await _roleManager.Roles.ToListAsync();
            model.AllRoles = rolesForView.Select(r => r.Name).ToList();
            model.UserRoles = model.SelectedRoles ?? new List<string>();
            return View(model);
        }

        // GET: Admin/Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            ViewData["PageTitle"] = "Delete User";
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var viewModel = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList()
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser.Id == user.Id)
                {
                    _logger.LogWarning("Admin {AdminId} attempted to delete their own account", currentUser.Id);
                    TempData["ErrorMessage"] = "You cannot delete your own account!";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} ({Email}) deleted by admin {AdminId}",
                        user.Id, user.Email, currentUser.Id);
                    TempData["SuccessMessage"] = "User deleted successfully!";
                }
                else
                {
                    _logger.LogError("Failed to delete user {UserId}: {Errors}",
                        user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
                    TempData["ErrorMessage"] = "Failed to delete user.";
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
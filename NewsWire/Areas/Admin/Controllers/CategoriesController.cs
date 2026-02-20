using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsWire.Models;
using NewsWire.Services.Interfaces;

namespace NewsWire.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["PageTitle"] = "Categories Management";
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }

        public async Task<IActionResult> Details(int? id)
        {
            ViewData["PageTitle"] = "Category Details";
            if (id == null)
                return NotFound();

            var category = await _categoryService.GetCategoryByIdAsync(id.Value);
            if (category == null)
                return NotFound();

            return View(category);
        }

        public IActionResult Create()
        {
            ViewData["PageTitle"] = "Create Category";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Category category)
        {
            if (ModelState.IsValid)
            {
                var success = await _categoryService.CreateCategoryAsync(category);
                if (success)
                {
                    TempData["SuccessMessage"] = "Category created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(category);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            ViewData["PageTitle"] = "Edit Category";
            if (id == null)
                return NotFound();

            var category = await _categoryService.GetCategoryByIdAsync(id.Value);
            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Category category)
        {
            if (id != category.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var success = await _categoryService.UpdateCategoryAsync(category);
                if (success)
                {
                    TempData["SuccessMessage"] = "Category updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                if (!await _categoryService.CategoryExistsAsync(category.Id))
                    return NotFound();
            }
            return View(category);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            ViewData["PageTitle"] = "Delete Category";
            if (id == null)
                return NotFound();

            var category = await _categoryService.GetCategoryByIdAsync(id.Value);
            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _categoryService.DeleteCategoryAsync(id);
            if (success)
                TempData["SuccessMessage"] = "Category deleted successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}

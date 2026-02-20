using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsWire.Models;
using NewsWire.Services.Interfaces;

namespace NewsWire.Controllers
{
    public class CategoriesController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(
            ICategoryService categoryService,
            ILogger<CategoriesController> logger) : base(logger)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(Index));
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return HandleNullId(nameof(Details));

            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id.Value);

                if (category == null)
                    return HandleNotFound("Category", id.Value);

                ViewBag.NewsCount = await _categoryService.GetNewsCountByCategoryAsync(id.Value);
                return View(category);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(Details));
            }
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            try
            {
                var success = await _categoryService.CreateCategoryAsync(category);

                if (success)
                {
                    SetSuccessMessage($"Category '{category.Name}' created successfully!");
                    return RedirectToAction(nameof(Index));
                }

                SetErrorMessage("Category name already exists. Please choose a different name.");
                return View(category);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(Create));
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return HandleNullId(nameof(Edit));

            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id.Value);

                if (category == null)
                    return HandleNotFound("Category", id.Value);

                return View(category);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(Edit));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Category category)
        {
            if (id != category.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(category);

            try
            {
                var success = await _categoryService.UpdateCategoryAsync(category);

                if (success)
                {
                    SetSuccessMessage($"Category '{category.Name}' updated successfully!");
                    return RedirectToAction(nameof(Index));
                }

                SetErrorMessage("Failed to update category. The category name may already exist.");
                return View(category);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(Edit));
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return HandleNullId(nameof(Delete));

            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id.Value);

                if (category == null)
                    return HandleNotFound("Category", id.Value);

                ViewBag.NewsCount = await _categoryService.GetNewsCountByCategoryAsync(id.Value);
                return View(category);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(Delete));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                    return HandleNotFound("Category", id);

                var success = await _categoryService.DeleteCategoryAsync(id);

                if (success)
                {
                    SetSuccessMessage($"Category '{category.Name}' deleted successfully!");
                    return RedirectToAction(nameof(Index));
                }

                SetErrorMessage("Cannot delete category. It may contain news articles.");
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(DeleteConfirmed));
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using NewsWire.Models;
using NewsWire.Services;

namespace NewsWire.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public HomeController(
            ICategoryService categoryService,
            ILogger<HomeController> logger) : base(logger)
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
                _logger.LogError(ex, "Error loading home page");
                return View(new List<Category>()); // Return empty list on error
            }
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError("Error page displayed for request: {RequestId}", requestId);
            return View(new ErrorViewModel { RequestId = requestId });
        }
    }
}
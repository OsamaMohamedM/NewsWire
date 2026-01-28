using Microsoft.AspNetCore.Mvc;

namespace NewsWire.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly ILogger _logger;

        protected BaseController(ILogger logger)
        {
            _logger = logger;
        }

        protected void SetSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
            _logger.LogInformation("Success: {Message}", message);
        }

        protected void SetErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
            _logger.LogWarning("Error: {Message}", message);
        }

        protected void SetInfoMessage(string message)
        {
            TempData["InfoMessage"] = message;
            _logger.LogInformation("Info: {Message}", message);
        }

        protected IActionResult HandleNullId(string actionName)
        {
            _logger.LogWarning("{Action}: ID was null", actionName);
            SetErrorMessage("Invalid ID provided.");
            return NotFound();
        }

        protected IActionResult HandleNotFound(string entityName, int id)
        {
            _logger.LogWarning("{Entity} with ID {Id} not found", entityName, id);
            SetErrorMessage($"{entityName} not found.");
            return NotFound();
        }

        protected IActionResult HandleValidationError(string actionName)
        {
            _logger.LogWarning("{Action}: Model validation failed", actionName);
            SetErrorMessage("Please correct the errors and try again.");
            return View();
        }

        protected IActionResult HandleException(Exception ex, string actionName)
        {
            _logger.LogError(ex, "Error in {Action}", actionName);
            SetErrorMessage("An error occurred. Please try again later.");
            return RedirectToAction("Error", "Home");
        }

        protected string? GetCurrentUserId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        protected bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        protected IActionResult HandleUnauthorized(string actionName)
        {
            _logger.LogWarning("Unauthorized access attempt to {Action} by user {UserId}",
                actionName, GetCurrentUserId());
            SetErrorMessage("You don't have permission to perform this action.");
            return RedirectToAction("Index", "Home");
        }
    }
}
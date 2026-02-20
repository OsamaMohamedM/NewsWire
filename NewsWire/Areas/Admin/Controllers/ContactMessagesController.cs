using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsWire.Services.Interfaces;

namespace NewsWire.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ContactMessagesController : Controller
    {
        private readonly IContactService _contactService;

        public ContactMessagesController(IContactService contactService)
        {
            _contactService = contactService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["PageTitle"] = "Contact Messages";
            var messages = await _contactService.GetAllMessagesAsync();
            return View(messages);
        }

        public async Task<IActionResult> Details(int? id)
        {
            ViewData["PageTitle"] = "Message Details";
            if (id == null)
                return NotFound();

            var contactUs = await _contactService.GetMessageByIdAsync(id.Value);
            if (contactUs == null)
                return NotFound();

            return View(contactUs);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            ViewData["PageTitle"] = "Delete Message";
            if (id == null)
                return NotFound();

            var contactUs = await _contactService.GetMessageByIdAsync(id.Value);
            if (contactUs == null)
                return NotFound();

            return View(contactUs);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _contactService.DeleteMessageAsync(id);
            if (success)
                TempData["SuccessMessage"] = "Contact message deleted successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}

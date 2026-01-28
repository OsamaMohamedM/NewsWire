using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsWire.Models;
using NewsWire.Services;

namespace NewsWire.Controllers
{
    public class ContactUsController : BaseController
    {
        private readonly IContactService _contactService;

        public ContactUsController(
            IContactService contactService,
            ILogger<ContactUsController> logger) : base(logger)
        {
            _contactService = contactService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("Id,Name,Email,Subject,Message")] ContactUs contactUs)
        {
            if (!ModelState.IsValid)
                return View(contactUs);

            try
            {
                var success = await _contactService.CreateMessageAsync(contactUs);

                if (success)
                {
                    SetSuccessMessage("Thank you for contacting us! We'll get back to you soon.");
                    return RedirectToAction(nameof(Index));
                }

                SetErrorMessage("Failed to send message. Please check your email and try again.");
                return View(contactUs);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(Index));
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return HandleNullId(nameof(Details));

            try
            {
                var message = await _contactService.GetMessageByIdAsync(id.Value);

                if (message == null)
                    return HandleNotFound("Contact Message", id.Value);

                await _contactService.MarkAsReadAsync(id.Value);
                return View(message);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(Details));
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return HandleNullId(nameof(Delete));

            try
            {
                var message = await _contactService.GetMessageByIdAsync(id.Value);

                if (message == null)
                    return HandleNotFound("Contact Message", id.Value);

                return View(message);
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
                var success = await _contactService.DeleteMessageAsync(id);

                if (success)
                {
                    SetSuccessMessage("Contact message deleted successfully!");
                }
                else
                {
                    SetErrorMessage("Failed to delete message.");
                }

                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(DeleteConfirmed));
            }
        }
    }
}

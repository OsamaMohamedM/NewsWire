using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsWire.Data;
using NewsWire.Models;

namespace NewsWire.Controllers
{
    public class ContactUsController : Controller
    {
        private readonly NewsDbContext _context;

        public ContactUsController(NewsDbContext context)
        {
            _context = context;
        }

        // GET: ContactUs
        public IActionResult Index()
        {
            return View();
        }

        // GET: ContactUs/Details/5
        [Authorize]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contactUs = _context.ContactUs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contactUs == null)
            {
                return NotFound();
            }

            return View(contactUs);
        }

        // GET: ContactUs/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: ContactUs/Create To protect from overposting attacks, enable the specific
        // properties you want to bind to. For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Create([Bind("Id,Name,Email,Subject,Message")] ContactUs contactUs)
        {
            if (ModelState.IsValid)
            {
                _context.Add(contactUs);
                _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(contactUs);
        }

        // GET: ContactUs/Edit/5
        [Authorize]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contactUs = _context.ContactUs.Find(id);
            if (contactUs == null)
            {
                return NotFound();
            }
            return View(contactUs);
        }

        // GET: ContactUs/Delete/5
        [Authorize("Admin")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contactUs = _context.ContactUs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contactUs == null)
            {
                return NotFound();
            }

            return View(contactUs);
        }

        // POST: ContactUs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize("Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            var contactUs = _context.ContactUs.Find(id);
            if (contactUs != null)
            {
                _context.ContactUs.Remove(contactUs);
            }

            _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public bool ContactUsExists(int id)
        {
            return _context.ContactUs.Any(e => e.Id == id);
        }
    }
}
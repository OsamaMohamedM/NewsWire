using Microsoft.EntityFrameworkCore;
using NewsWire.Data;
using NewsWire.Models;

namespace NewsWire.Services
{
    public class ContactService : IContactService
    {
        private readonly NewsDbContext _context;
        private readonly ILogger<ContactService> _logger;

        public ContactService(NewsDbContext context, ILogger<ContactService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ContactUs>> GetAllMessagesAsync()
        {
            try
            {
                return await _context.ContactUs
                    .OrderByDescending(c => c.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contact messages");
                return new List<ContactUs>();
            }
        }

        public async Task<ContactUs?> GetMessageByIdAsync(int id)
        {
            try
            {
                return await _context.ContactUs
                    .FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contact message with ID: {MessageId}", id);
                return null;
            }
        }

        public async Task<bool> CreateMessageAsync(ContactUs message)
        {
            try
            {
                if (message == null)
                {
                    _logger.LogWarning("Attempted to create null contact message");
                    return false;
                }

                // Validate email format
                if (string.IsNullOrWhiteSpace(message.Email) || !message.Email.Contains("@"))
                {
                    _logger.LogWarning("Invalid email format: {Email}", message.Email);
                    return false;
                }

                _context.ContactUs.Add(message);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Contact message created from {Email}", message.Email);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contact message");
                return false;
            }
        }

        public async Task<bool> DeleteMessageAsync(int id)
        {
            try
            {
                var message = await _context.ContactUs.FindAsync(id);
                if (message == null)
                {
                    _logger.LogWarning("Contact message not found for deletion: {MessageId}", id);
                    return false;
                }

                _context.ContactUs.Remove(message);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Contact message deleted: {MessageId}", id);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contact message: {MessageId}", id);
                return false;
            }
        }

        public async Task<bool> MessageExistsAsync(int id)
        {
            try
            {
                return await _context.ContactUs.AnyAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking message existence: {MessageId}", id);
                return false;
            }
        }

        public async Task<int> GetUnreadMessagesCountAsync()
        {
            try
            {
                return await _context.ContactUs.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting unread messages");
                return 0;
            }
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            try
            {
                var message = await _context.ContactUs.FindAsync(id);
                if (message == null)
                {
                    _logger.LogWarning("Message not found for marking as read: {MessageId}", id);
                    return false;
                }

                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("Message marked as read: {MessageId}", id);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message as read: {MessageId}", id);
                return false;
            }
        }
    }
}
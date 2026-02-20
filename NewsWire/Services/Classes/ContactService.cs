using NewsWire.Models;
using NewsWire.Repositories.Interfaces;
using NewsWire.Services.Interfaces;

namespace NewsWire.Services.Classes
{
    public class ContactService : IContactService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ContactService> _logger;

        public ContactService(IUnitOfWork unitOfWork, ILogger<ContactService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<ContactUs>> GetAllMessagesAsync()
        {
            try
            {
                var messages = await _unitOfWork.Contacts.GetAllOrderedAsync();
                return messages.ToList();
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
                return await _unitOfWork.Contacts.GetByIdAsync(id);
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
                    return false;

                if (string.IsNullOrWhiteSpace(message.Email) || !message.Email.Contains('@'))
                    return false;

                await _unitOfWork.Contacts.AddAsync(message);
                var result = await _unitOfWork.SaveChangesAsync();
                return result > 0;
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
                var message = await _unitOfWork.Contacts.GetByIdAsync(id);
                if (message == null)
                    return false;

                _unitOfWork.Contacts.Remove(message);
                var result = await _unitOfWork.SaveChangesAsync();
                return result > 0;
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
                return await _unitOfWork.Contacts.ExistsAsync(c => c.Id == id);
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
                return await _unitOfWork.Contacts.CountAsync();
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
                var message = await _unitOfWork.Contacts.GetByIdAsync(id);
                if (message == null)
                    return false;

                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message as read: {MessageId}", id);
                return false;
            }
        }
    }
}

using NewsWire.Models;

namespace NewsWire.Services.Interfaces
{
    public interface IContactService
    {
        Task<List<ContactUs>> GetAllMessagesAsync();
        Task<ContactUs?> GetMessageByIdAsync(int id);
        Task<bool> CreateMessageAsync(ContactUs message);
        Task<bool> DeleteMessageAsync(int id);
        Task<bool> MessageExistsAsync(int id);
        Task<int> GetUnreadMessagesCountAsync();
        Task<bool> MarkAsReadAsync(int id);
    }
}

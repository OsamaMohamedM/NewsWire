namespace NewsWire.Services.Interfaces
{
    public interface IFileUploadService
    {
        Task<string?> UploadImageAsync(IFormFile file, string folder);
        Task<bool> DeleteImageAsync(string imagePath);
        bool ValidateImageFile(IFormFile file);
    }
}

using Microsoft.AspNetCore.Http;

namespace NewsWire.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder);

        Task<bool> DeleteImageAsync(string imagePath);

        bool ValidateImageFile(IFormFile file);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<FileUploadService> _logger;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024;

        public FileUploadService(IWebHostEnvironment webHostEnvironment, ILogger<FileUploadService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Attempted to upload null or empty file");
                    return null;
                }

                if (!ValidateImageFile(file))
                {
                    _logger.LogWarning("Invalid file type or size: {FileName}", file.FileName);
                    return null;
                }

                string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", folder);
                Directory.CreateDirectory(folderPath);

                string fileExtension = Path.GetExtension(file.FileName);
                string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                string filePath = Path.Combine(folderPath, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return $"/Images/{folder}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", file?.FileName);
                return null;
            }
        }

        public async Task<bool> DeleteImageAsync(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath) || imagePath.Contains("default"))
                {
                    return true;
                }

                string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));

                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    _logger.LogInformation("Deleted image: {ImagePath}", imagePath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {ImagePath}", imagePath);
                return false;
            }
        }

        public bool ValidateImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > MaxFileSize)
            {
                _logger.LogWarning("File too large: {Size} bytes", file.Length);
                return false;
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Invalid file extension: {Extension}", extension);
                return false;
            }

            var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
            {
                _logger.LogWarning("Invalid content type: {ContentType}", file.ContentType);
                return false;
            }

            return true;
        }
    }
}
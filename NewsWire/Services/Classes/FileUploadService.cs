using NewsWire.Services.Interfaces;

namespace NewsWire.Services.Classes
{
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

        public async Task<string?> UploadImageAsync(IFormFile file, string folder)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return null;

                if (!ValidateImageFile(file))
                    return null;

                if (string.IsNullOrEmpty(_webHostEnvironment.ContentRootPath))
                {
                    _logger.LogError("ContentRootPath is null or empty");
                    return null;
                }

                string uploadsBasePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Uploads");
                string folderPath = Path.Combine(uploadsBasePath, folder);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                string filePath = Path.Combine(folderPath, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                {
                    await file.CopyToAsync(fileStream);
                    await fileStream.FlushAsync();
                }

                if (!File.Exists(filePath))
                {
                    _logger.LogError("File was not created: {FilePath}", filePath);
                    return null;
                }

                return $"/uploads/{folder}/{uniqueFileName}";
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
                    return true;

                string? fullPath = null;

                if (imagePath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
                {
                    fullPath = Path.Combine(
                        _webHostEnvironment.ContentRootPath,
                        imagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                }
                else if (!string.IsNullOrEmpty(_webHostEnvironment.WebRootPath))
                {
                    fullPath = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        imagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                }

                if (string.IsNullOrEmpty(fullPath))
                    return false;

                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
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
                return false;

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return false;

            var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
                return false;

            return true;
        }
    }
}
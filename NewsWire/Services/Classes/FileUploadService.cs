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

            if (!IsValidImageByMagicNumber(file))
            {
                _logger.LogWarning("File rejected due to invalid magic number: {FileName}", file.FileName);
                return false;
            }

            return true;
        }

        private bool IsValidImageByMagicNumber(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                var buffer = new byte[8];
                stream.Read(buffer, 0, 8);
                stream.Position = 0;

                if (buffer.Length < 2)
                    return false;

                if (buffer[0] == 0xFF && buffer[1] == 0xD8)
                    return true;

                if (buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47)
                    return true;

                if (buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46)
                    return true;

                if (buffer.Length >= 12 &&
                    buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46 &&
                    buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating file magic number");
                return false;
            }
        }
    }
}
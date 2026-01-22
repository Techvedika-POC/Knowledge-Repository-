using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;


namespace Knowledge_Repository.Application.Implementations.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _uploadsRoot;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(string rootPath, ILogger<FileStorageService> logger)
        {
            if (string.IsNullOrWhiteSpace(rootPath))
                throw new ArgumentException("rootPath must be supplied", nameof(rootPath));

            _uploadsRoot = rootPath;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Directory.CreateDirectory(_uploadsRoot);
        }

        public async Task<string> SaveFileAsync(byte[] fileData, string originalFileName)
        {
            if (fileData == null || fileData.Length == 0)
                throw new ArgumentException("fileData cannot be empty", nameof(fileData));

            var safeName = Path.GetFileName(originalFileName ?? "file");
            var storageFileName = $"{Guid.NewGuid()}_{safeName}";
            var physicalPath = Path.Combine(_uploadsRoot, storageFileName);

            try
            {
                await File.WriteAllBytesAsync(physicalPath, fileData);
                _logger?.LogInformation("Saved file {PhysicalPath} (original: {OriginalName})", physicalPath, originalFileName);
                return $"/uploads/{storageFileName}";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving file {OriginalName} to {Path}", originalFileName, physicalPath);
                throw;
            }
        }

        public Task<bool> DeleteFileAsync(string publicPathOrFileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(publicPathOrFileName))
                    return Task.FromResult(false);

                var fileName = Path.GetFileName(publicPathOrFileName.Trim());
                if (string.IsNullOrWhiteSpace(fileName)) return Task.FromResult(false);

                var fullPath = Path.Combine(_uploadsRoot, fileName);

                var resolved = Path.GetFullPath(fullPath);
                var rootResolved = Path.GetFullPath(_uploadsRoot);

                if (!resolved.StartsWith(rootResolved, StringComparison.OrdinalIgnoreCase))
                {
                    _logger?.LogWarning("Refused to delete outside of uploads: {Path}", resolved);
                    return Task.FromResult(false);
                }

                if (File.Exists(resolved))
                {
                    File.Delete(resolved);
                    _logger?.LogInformation("Deleted file {Path}", resolved);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to delete file {Path}", publicPathOrFileName);
                return Task.FromResult(false);
            }
        }
   
    }
}

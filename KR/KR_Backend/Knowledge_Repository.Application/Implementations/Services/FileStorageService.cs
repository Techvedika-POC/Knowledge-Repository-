using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
  
    public class FileStorageService : IFileStorageService
    {
        private readonly string _rootPath;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(ILogger<FileStorageService> logger)
        {
            _logger = logger;
            _rootPath = Path.Combine(AppContext.BaseDirectory, "uploads");
            Directory.CreateDirectory(_rootPath);
        }

        public async Task<string> SaveFileAsync(byte[] fileData, string fileName)
        {
            try
            {
                var uniqueName = $"{Guid.NewGuid()}_{fileName}";
                var filePath = Path.Combine(_rootPath, uniqueName);

                await File.WriteAllBytesAsync(filePath, fileData);

                return $"/uploads/{uniqueName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file {FileName}", fileName);
                throw;
            }
        }

        public Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_rootPath, Path.GetFileName(filePath));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FilePath}", filePath);
                return Task.FromResult(false);
            }
        }
    }
}

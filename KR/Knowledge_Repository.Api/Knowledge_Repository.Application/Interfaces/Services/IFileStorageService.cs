using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    /// <summary>
    /// Abstracts file system or cloud storage for knowledge item attachments.
    /// </summary>
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(byte[] fileData, string fileName);
        Task<bool> DeleteFileAsync(string filePath);
    }
}

using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(byte[] fileData, string fileName);
        Task<bool> DeleteFileAsync(string filePath);
    }
}

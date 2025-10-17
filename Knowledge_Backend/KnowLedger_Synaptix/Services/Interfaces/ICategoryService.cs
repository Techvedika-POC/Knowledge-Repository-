using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto?> GetCategoryByIdAsync(Guid categoryId);
        Task<CategoryDto?> GetCategoryByNameAsync(string categoryName);
    }
}

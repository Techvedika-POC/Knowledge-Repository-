using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(Guid categoryId);
        Task<Category?> GetCategoryByNameAsync(string categoryName);
    }
}

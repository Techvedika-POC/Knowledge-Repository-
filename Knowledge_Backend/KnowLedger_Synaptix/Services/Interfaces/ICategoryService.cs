using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface ICategoryService
    {
        /// <summary>
        /// Retrieves all categories available.
        /// </summary>
        Task<List<Category>> GetAllCategoriesAsync();

        /// <summary>
        /// Retrieves a category by its unique identifier.
        /// </summary>
        Task<Category?> GetCategoryByIdAsync(Guid categoryId);

        /// <summary>
        /// Retrieves a category by its name.
        /// </summary>
        Task<Category?> GetCategoryByNameAsync(string categoryName);
    }
}

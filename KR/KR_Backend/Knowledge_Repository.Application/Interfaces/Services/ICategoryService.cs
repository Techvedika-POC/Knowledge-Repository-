using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface ICategoryService
    {
   
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(Guid categoryId);
        Task<Category?> GetCategoryByNameAsync(string categoryName);
        Task AddCategoryAsync(Category category);
        Task<bool> UpdateCategoryAsync(Guid id, Category updatedCategory);
        Task<bool> DeleteCategoryAsync(Guid id);
    }
}

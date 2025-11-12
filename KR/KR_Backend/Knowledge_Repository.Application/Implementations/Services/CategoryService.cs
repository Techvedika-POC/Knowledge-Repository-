using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.ToList();
        }


        public async Task<Category?> GetCategoryByIdAsync(Guid categoryId)
        {
            return await _categoryRepository.GetByIdAsync(categoryId);
        }

        public async Task<Category?> GetCategoryByNameAsync(string categoryName)
        {
            return await _categoryRepository.GetByNameAsync(categoryName);
        }
        public async Task AddCategoryAsync(Category category)
        {
            category.CategoryId = Guid.NewGuid();
            category.CreatedOn = DateTime.UtcNow;
            await _categoryRepository.AddAsync(category);
        }

        public async Task<bool> UpdateCategoryAsync(Guid id, Category updatedCategory)
        {
            var existing = await _categoryRepository.GetByIdAsync(id);
            if (existing == null) return false;

            existing.CategoryName = updatedCategory.CategoryName;
            existing.Description = updatedCategory.Description;
            existing.DomainId = updatedCategory.DomainId;
            existing.UpdatedOn = DateTime.UtcNow;

            await _categoryRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var existing = await _categoryRepository.GetByIdAsync(id);
            if (existing == null) return false;

            await _categoryRepository.DeleteAsync(existing);
            return true;
        }

    }
}


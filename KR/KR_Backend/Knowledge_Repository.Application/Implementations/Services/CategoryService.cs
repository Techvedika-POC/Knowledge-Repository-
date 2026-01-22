using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;

namespace Knowledge_Repository.Application.Implementations.Services
{
    /// <summary>
    /// Handles category-related operations such as creation, retrieval, update, and deletion.
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository
                ?? throw new ArgumentNullException(
                    nameof(categoryRepository),
                    "Category repository dependency cannot be null.");
        }

        /// <summary>
        /// Retrieves all available categories.
        /// </summary>
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync()
                ?? throw new InvalidOperationException(
                    "Failed to retrieve categories. Data source returned null.");

            return categories.ToList();
        }

        /// <summary>
        /// Retrieves a category by its unique identifier.
        /// </summary>
        public async Task<Category?> GetCategoryByIdAsync(Guid categoryId)
        {
            if (categoryId == Guid.Empty)
                throw new ArgumentException(
                    "Category ID cannot be empty.",
                    nameof(categoryId));

            return await _categoryRepository.GetByIdAsync(categoryId);
        }

        /// <summary>
        /// Retrieves a category by its name.
        /// </summary>
        public async Task<Category?> GetCategoryByNameAsync(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                throw new ArgumentException(
                    "Category name cannot be null or empty.",
                    nameof(categoryName));

            return await _categoryRepository.GetByNameAsync(categoryName.Trim());
        }

        /// <summary>
        /// Adds a new category to the system.
        /// </summary>
        public async Task AddCategoryAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(
                    nameof(category),
                    "Category data cannot be null.");

            if (string.IsNullOrWhiteSpace(category.CategoryName))
                throw new ArgumentException(
                    "Category name is required.",
                    nameof(category.CategoryName));

            category.CategoryId = Guid.NewGuid();
            category.CreatedOn = DateTime.UtcNow;

            await _categoryRepository.AddAsync(category);
        }

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        public async Task<bool> UpdateCategoryAsync(Guid id, Category updatedCategory)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(
                    "Category ID cannot be empty.",
                    nameof(id));

            if (updatedCategory == null)
                throw new ArgumentNullException(
                    nameof(updatedCategory),
                    "Updated category data cannot be null.");

            var existing = await _categoryRepository.GetByIdAsync(id);

            if (existing == null)
                throw new KeyNotFoundException(
                    $"No category found with the ID '{id}'.");

            existing.CategoryName = updatedCategory.CategoryName;
            existing.Description = updatedCategory.Description;
            existing.DomainId = updatedCategory.DomainId;
            existing.UpdatedOn = DateTime.UtcNow;

            await _categoryRepository.UpdateAsync(existing);
            return true;
        }

        /// <summary>
        /// Deletes a category by its identifier.
        /// </summary>
        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(
                    "Category ID cannot be empty.",
                    nameof(id));

            var existing = await _categoryRepository.GetByIdAsync(id);

            if (existing == null)
                throw new KeyNotFoundException(
                    $"Cannot delete category. No category exists with the ID '{id}'.");

            await _categoryRepository.DeleteAsync(existing);
            return true;
        }
    }
}

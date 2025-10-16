using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowLedger_Synaptix.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public CategoryService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }
        //Get all categories
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Select(c => new Category
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    CreatedOn = c.CreatedOn,
                    UpdatedOn = c.UpdatedOn
                })
                .ToListAsync();
        }
        //get category by id
        public async Task<Category?> GetCategoryByIdAsync(Guid categoryId)
        {
            return await _context.Categories
                .Where(c => c.CategoryId == categoryId)
                .Select(c => new Category
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    CreatedOn = c.CreatedOn,
                    UpdatedOn = c.UpdatedOn
                })
                .FirstOrDefaultAsync();
        }
        //get category by name
        public async Task<Category?> GetCategoryByNameAsync(string categoryName)
        {
            return await _context.Categories
                .Where(c => c.CategoryName.ToLower() == categoryName.ToLower()) 
                .Select(c => new Category
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    CreatedOn = c.CreatedOn,
                    UpdatedOn = c.UpdatedOn
                })
                .FirstOrDefaultAsync();
        }
    }
}

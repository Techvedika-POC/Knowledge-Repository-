// Infrastructure/Repositories/CategoryRepository.cs
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public CategoryRepository(Knowledge_Repository_dbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Category?> GetByNameAsync(string categoryName)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == categoryName.ToLower());
        }
    }
}

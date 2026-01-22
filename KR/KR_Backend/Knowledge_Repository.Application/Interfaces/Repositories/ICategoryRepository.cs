
using Knowledge_Repository.Domain.Entities;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<Category?> GetByNameAsync(string categoryName);
    }
}

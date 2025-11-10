using Knowledge_Repository.Domain.Entities;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<Role?> GetByNameAsync(string roleName);
    }
}

using Knowledge_Repository.Domain.Entities;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IUserRoleRepository : IGenericRepository<UserRole>
    {
        Task<List<UserRole>> GetUserRolesAsync(Guid userId);
        Task AssignRoleAsync(Guid userId, Guid roleId);
        Task RemoveUserRoleAsync(Guid userId, Guid roleId);
    }
}

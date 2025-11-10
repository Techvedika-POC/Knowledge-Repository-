using Knowledge_Repository.Domain.Entities;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IRoleService
    {
        Task<List<Role>> GetAllRolesAsync();
        Task<Role?> GetRoleByIdAsync(Guid roleId);
        Task<Role?> GetRoleByNameAsync(string roleName);
        Task<Role> CreateRoleAsync(string roleName, string description);
        Task<Role> UpdateRoleAsync(Guid roleId, string roleName, string description);
        Task DeleteRoleAsync(Guid roleId);
    }
}

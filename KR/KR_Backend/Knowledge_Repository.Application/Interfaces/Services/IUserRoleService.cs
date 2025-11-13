using Knowledge_Repository.Domain.Entities;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IUserRoleService
    {
        Task<List<Role>> GetRolesForUserAsync(Guid userId);
        Task AssignRoleToUserAsync(Guid userId, Guid roleId);
        Task RemoveRoleFromUserAsync(Guid userId, Guid roleId);
    }
}

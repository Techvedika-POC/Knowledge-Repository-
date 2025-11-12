using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IRoleRepository _roleRepository;

        public UserRoleService(IUserRoleRepository userRoleRepository, IRoleRepository roleRepository)
        {
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
        }

        public async Task<List<Role>> GetRolesForUserAsync(Guid userId)
        {
            var roles = await _userRoleRepository.GetUserRolesAsync(userId);
            return roles.Select(ur => ur.Role).ToList();
        }

        public async Task AssignRoleToUserAsync(Guid userId, Guid roleId)
        {
            await _userRoleRepository.AssignRoleAsync(userId, roleId);
        }

        public async Task RemoveRoleFromUserAsync(Guid userId, Guid roleId)
        {
            await _userRoleRepository.RemoveUserRoleAsync(userId, roleId);
        }
    }
}

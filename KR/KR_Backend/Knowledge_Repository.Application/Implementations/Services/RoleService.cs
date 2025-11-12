using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
namespace Knowledge_Repository.Application.Implementations.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<List<Role>> GetAllRolesAsync()
     => (await _roleRepository.GetAllAsync()).ToList();


        public async Task<Role?> GetRoleByIdAsync(Guid roleId)
            => await _roleRepository.GetByIdAsync(roleId);

        public async Task<Role?> GetRoleByNameAsync(string roleName)
            => await _roleRepository.GetByNameAsync(roleName);

        public async Task<Role> CreateRoleAsync(string roleName, string description)
        {
            var existing = await _roleRepository.GetByNameAsync(roleName);
            if (existing != null)
                throw new InvalidOperationException("Role already exists.");

            var role = new Role
            {
                RoleId = Guid.NewGuid(),
                RoleName = roleName,
                Description = description,
                CreatedOn = DateTime.UtcNow
            };

            await _roleRepository.AddAsync(role);
            return role;
        }

        public async Task<Role> UpdateRoleAsync(Guid roleId, string roleName, string description)
        {
            var role = await _roleRepository.GetByIdAsync(roleId)
                ?? throw new KeyNotFoundException("Role not found.");

            role.RoleName = roleName;
            role.Description = description;
            role.UpdatedOn = DateTime.UtcNow;

            await _roleRepository.UpdateAsync(role);
            return role;
        }

        public async Task DeleteRoleAsync(Guid roleId)
        {
            var role = await _roleRepository.GetByIdAsync(roleId)
                ?? throw new KeyNotFoundException("Role not found.");

            await _roleRepository.DeleteAsync(role);
        }

    }
}

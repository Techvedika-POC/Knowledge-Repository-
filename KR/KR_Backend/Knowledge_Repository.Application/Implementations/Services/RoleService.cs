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
            _roleRepository = roleRepository
                ?? throw new ArgumentNullException(nameof(roleRepository),
                    "Role repository dependency cannot be null.");
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();

            if (roles == null)
                throw new InvalidOperationException(
                    "Failed to retrieve roles. The data source returned no results.");

            return roles.ToList();
        }

        public async Task<Role?> GetRoleByIdAsync(Guid roleId)
        {
            if (roleId == Guid.Empty)
                throw new ArgumentException(
                    "Role ID cannot be empty. Please provide a valid role identifier.",
                    nameof(roleId));

            return await _roleRepository.GetByIdAsync(roleId);
        }

        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException(
                    "Role name cannot be null or empty. Please provide a valid role name.",
                    nameof(roleName));

            return await _roleRepository.GetByNameAsync(roleName.Trim());
        }

        public async Task<Role> CreateRoleAsync(string roleName, string description)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException(
                    "Role name is required and cannot be empty.",
                    nameof(roleName));

            var existingRole = await _roleRepository.GetByNameAsync(roleName.Trim());

            if (existingRole != null)
                throw new InvalidOperationException(
                    $"A role with the name '{roleName}' already exists. " +
                    $"Please choose a different role name.");

            var role = new Role
            {
                RoleId = Guid.NewGuid(),
                RoleName = roleName.Trim(),
                Description = description?.Trim(),
                CreatedOn = DateTime.UtcNow
            };

            await _roleRepository.AddAsync(role);

            return role;
        }

        public async Task<Role> UpdateRoleAsync(Guid roleId, string roleName, string description)
        {
            if (roleId == Guid.Empty)
                throw new ArgumentException(
                    "Role ID cannot be empty. A valid role identifier is required.",
                    nameof(roleId));

            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException(
                    "Role name cannot be null or empty.",
                    nameof(roleName));

            var role = await _roleRepository.GetByIdAsync(roleId);

            if (role == null)
                throw new KeyNotFoundException(
                    $"No role was found with the ID '{roleId}'. " +
                    $"Please verify the role ID and try again.");

            role.RoleName = roleName.Trim();
            role.Description = description?.Trim();
            role.UpdatedOn = DateTime.UtcNow;

            await _roleRepository.UpdateAsync(role);

            return role;
        }

        public async Task DeleteRoleAsync(Guid roleId)
        {
            if (roleId == Guid.Empty)
                throw new ArgumentException(
                    "Role ID cannot be empty. A valid role identifier is required.",
                    nameof(roleId));

            var role = await _roleRepository.GetByIdAsync(roleId);

            if (role == null)
                throw new KeyNotFoundException(
                    $"Cannot delete role. No role exists with the ID '{roleId}'.");

            await _roleRepository.DeleteAsync(role);
        }
    }
}

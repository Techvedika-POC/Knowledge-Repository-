using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class UserRoleRepository : GenericRepository<UserRole>, IUserRoleRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public UserRoleRepository(Knowledge_Repository_dbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<UserRole>> GetUserRolesAsync(Guid userId)
        {
            return await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();
        }

        public async Task AssignRoleAsync(Guid userId, Guid roleId)
        {
            if (!await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId))
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserRoleId = Guid.NewGuid(),
                    UserId = userId,
                    RoleId = roleId,
                    CreatedOn = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveUserRoleAsync(Guid userId, Guid roleId)
        {
            var existing = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (existing != null)
            {
                _context.UserRoles.Remove(existing);
                await _context.SaveChangesAsync();
            }
        }
    }
}

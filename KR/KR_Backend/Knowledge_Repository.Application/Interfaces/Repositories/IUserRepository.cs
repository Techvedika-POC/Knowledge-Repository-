using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> ExistsByEmailAsync(string email);
        Task<User?> GetUserWithRolesByEmailAsync(string email);
        Task<string?> GetUserNameAsync(Guid userId);
        Task<List<User>> GetUsersByIdsAsync(List<Guid> userIds);
    }
}

using Knowledge_Repository.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IPasswordResetRepository : IGenericRepository<PasswordReset>
    {
        Task<PasswordReset?> GetLatestByUserIdAsync(Guid userId);
        Task<PasswordReset?> GetByResetTokenHashAsync(string resetTokenHash);
        Task<PasswordReset?> GetValidOtpAsync(Guid userId, string codeHash);
    }
}

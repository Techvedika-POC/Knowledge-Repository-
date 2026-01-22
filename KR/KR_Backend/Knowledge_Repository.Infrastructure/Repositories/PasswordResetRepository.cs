using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class PasswordResetRepository
     : GenericRepository<PasswordReset>, IPasswordResetRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public PasswordResetRepository(Knowledge_Repository_dbContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<PasswordReset?> GetLatestByUserIdAsync(Guid userId)
        {
            return await _context.PasswordResets
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedOn)
                .FirstOrDefaultAsync();
        }

        public async Task<PasswordReset?> GetByResetTokenHashAsync(string tokenHash)
        {
            return await _context.PasswordResets
                .Where(p => p.ResetTokenHash == tokenHash && !p.ResetTokenUsed)
                .FirstOrDefaultAsync();
        }

        public async Task<PasswordReset?> GetValidOtpAsync(Guid userId, string codeHash)
        {
            return await _context.PasswordResets
                .Where(p =>
                    p.UserId == userId &&
                    p.CodeHash == codeHash &&
                    !p.CodeUsed &&
                    p.CodeExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();
        }
    }

}
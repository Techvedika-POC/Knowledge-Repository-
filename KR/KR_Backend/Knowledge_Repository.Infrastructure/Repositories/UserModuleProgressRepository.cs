using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class UserModuleProgressRepository
     : IUserModuleProgressRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public UserModuleProgressRepository(
            Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        public async Task<UserModuleProgress?> GetAsync(
            Guid userId, Guid moduleId)
        {
            return await _context.UserModuleProgresses
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.ModuleId == moduleId);
        }

        public async Task TouchAsync(Guid userId, Guid moduleId)
        {
            var progress = await GetAsync(userId, moduleId);

            if (progress == null)
            {
                progress = new UserModuleProgress
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ModuleId = moduleId,
                    Status = "InProgress",
                    StartedOn = DateTime.UtcNow
                };
                _context.UserModuleProgresses.Add(progress);
            }

            progress.LastAccessed = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<UserModuleProgress>> GetByWeekAsync(
           Guid userId,
           Guid weekId)
        {
            return await (
                from p in _context.UserModuleProgresses
                join m in _context.Modules on p.ModuleId equals m.ModuleId
                where p.UserId == userId && m.WeekId == weekId
                select p
            ).ToListAsync();
        }


        public async Task CompleteAsync(Guid userId, Guid moduleId)
        {
            var progress = await GetAsync(userId, moduleId);
            if (progress == null) return;

            progress.Status = "Completed";
            progress.CompletedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

}

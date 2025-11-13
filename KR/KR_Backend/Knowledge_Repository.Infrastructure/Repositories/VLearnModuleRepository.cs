using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class VLearnModuleRepository : GenericRepository<Module>, IVLearnModuleRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public VLearnModuleRepository(Knowledge_Repository_dbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Module>> GetModulesByTopicAndUserAsync(Guid topicId, Guid userId)
        {
            return await _context.Modules
                .Where(m => m.TopicId == topicId)
                .Include(m => m.UserModuleProgresses
                    .Where(ump => ump.UserId == userId)) 
                .ToListAsync();
        }

        public async Task<bool> UpdateTestStatusAsync(Guid moduleId, Guid userId, bool isCompleted)
        {
            var progress = await _context.UserModuleProgresses
                .FirstOrDefaultAsync(ump => ump.ModuleId == moduleId && ump.UserId == userId);

            string newStatus = isCompleted ? "Completed" : "Pending";

            if (progress == null)
            {

                progress = new UserModuleProgress
                {
                    ModuleId = moduleId,
                    UserId = userId,
                    TestStatus = newStatus,
                    StartedOn = DateTime.UtcNow
                };
                _context.UserModuleProgresses.Add(progress);
            }
            else
            {
                progress.TestStatus = newStatus;
                _context.UserModuleProgresses.Update(progress);
            }

            await _context.SaveChangesAsync();
            return true;
        }

    }
}

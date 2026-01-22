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
    public class ModuleRepository : GenericRepository<Module>, IModuleRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public ModuleRepository(Knowledge_Repository_dbContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Module>> GetByWeekIdAsync(Guid weekId)
        {
            return await _dbSet
                .Where(m => m.WeekId == weekId)
                .OrderBy(m => m.OrderNo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Module>> GetModulesByPlanIdAsync(Guid planId)
        {
            return await _dbSet
                .Include(m => m.Week)
                .Where(m => m.Week.PlanId == planId)
                .OrderBy(m => m.OrderNo)
                .ToListAsync();
        }

        public async Task<Module?> GetModuleFullAsync(Guid moduleId)
        {
            var module = await _dbSet
                .AsNoTracking() 
                .Include(m => m.Lessons.OrderBy(l => l.OrderIndex))
                .Include(m => m.Resources)
                .Include(m => m.Assessments)
                    .ThenInclude(a => a.AssessmentQuestions)
                .FirstOrDefaultAsync(m => m.ModuleId == moduleId);

            return module;
        }
        public async Task<bool> IsModuleUnlockedAsync(Guid moduleId, Guid userId)
        {
            var module = await _dbSet.Include(m => m.Week).FirstOrDefaultAsync(m => m.ModuleId == moduleId);
            if (module == null) return false;
            var firstModuleOrder = await _dbSet
                .Where(m => m.WeekId == module.WeekId)
                .MinAsync(m => m.OrderNo);

            if (module.OrderNo == firstModuleOrder)
                return true;
            var previousModule = await _dbSet
                .Where(m => m.WeekId == module.WeekId && m.OrderNo == module.OrderNo - 1)
                .FirstOrDefaultAsync();

            if (previousModule == null) return true;

            var progress = await _context.UserModuleProgresses
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.UserId == userId &&
                    p.ModuleId == previousModule.ModuleId &&
                    p.Status == "Completed");

            return progress != null;
        }
    }
}

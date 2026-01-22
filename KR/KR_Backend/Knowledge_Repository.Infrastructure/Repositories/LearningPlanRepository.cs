using Application.Interfaces;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Knowledge_Repository.Application.Interfaces.Repositories;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class LearningPlanRepository
        : GenericRepository<LearningPlan>, ILearningPlanRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public LearningPlanRepository(Knowledge_Repository_dbContext context)
            : base(context)
        {
            _context = context;
        }
        public async Task<LearningPlan?> GetPlanWithHierarchyAsync(Guid planId)
        {
            return await _context.LearningPlans
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Modules)
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Modules)
                        .ThenInclude(m => m.Lessons)
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Modules)
                        .ThenInclude(m => m.Assessments)
                            .ThenInclude(a => a.AssessmentQuestions)
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Modules)
                        .ThenInclude(m => m.Resources)
                .FirstOrDefaultAsync(x => x.PlanId == planId);
        }

        public async Task<LearningPlan?> GetPlanWithHierarchyFullAsync(Guid planId)
        {
            return await _context.LearningPlans
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Modules)
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Modules)
                        .ThenInclude(m => m.Lessons)
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Modules)
                        .ThenInclude(m => m.Assessments)
                            .ThenInclude(a => a.AssessmentQuestions)
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Modules)
                        .ThenInclude(m => m.Resources)
                .FirstOrDefaultAsync(p => p.PlanId == planId);
        }

        public async Task<bool> IsPlanCompletedByUser(Guid planId, Guid userId)
        {
            var moduleIds = await _context.Modules
                .Where(m => _context.Weeks
                    .Any(w => w.PlanId == planId && w.WeekId == m.WeekId))
                .Select(m => m.ModuleId)
                .ToListAsync();

            if (!moduleIds.Any())
                return false;

            var completedCount = await _context.UserModuleProgresses
                .Where(p => p.UserId == userId &&
                            moduleIds.Contains(p.ModuleId) &&
                            p.Status == "Completed")
                .CountAsync();

            return completedCount == moduleIds.Count;
        }
    }
}

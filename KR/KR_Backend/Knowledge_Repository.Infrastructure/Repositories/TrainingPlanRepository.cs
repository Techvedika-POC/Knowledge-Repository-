using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class TrainingPlanRepository : ITrainingPlanRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public TrainingPlanRepository(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(LearningPlan plan)
        {
            _context.LearningPlans.Add(plan);
            await _context.SaveChangesAsync();
        }

        public async Task<LearningPlan?> GetByIdAsync(Guid planId)
        {
            return await _context.LearningPlans
      .AsNoTracking()
      .Include(p => p.Weeks)
          .ThenInclude(w => w.Modules)
              .ThenInclude(m => m.Lessons)
      .Include(p => p.Weeks)
          .ThenInclude(w => w.Modules)
              .ThenInclude(m => m.Resources)
      .Include(p => p.Weeks)
          .ThenInclude(w => w.Modules)
              .ThenInclude(m => m.Assessments)
                  .ThenInclude(a => a.AssessmentQuestions) 
      .Include(p => p.Weeks)
          .ThenInclude(w => w.Assignments)
      .FirstOrDefaultAsync(p => p.PlanId == planId);

        }

        public async Task UpdateAsync(LearningPlan plan)
        {
            _context.LearningPlans.Update(plan);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid planId)
        {
            var plan = await _context.LearningPlans
                .Include(p => p.Weeks)
                .ThenInclude(w => w.Modules)
                .FirstOrDefaultAsync(p => p.PlanId == planId);

            if (plan == null)
                return;

            _context.LearningPlans.Remove(plan);
            await _context.SaveChangesAsync();
        }
    }
}

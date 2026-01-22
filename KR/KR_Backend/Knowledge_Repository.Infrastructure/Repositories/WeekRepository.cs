using Application.Interfaces;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class WeekRepository : GenericRepository<Week>, IWeekRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public WeekRepository(Knowledge_Repository_dbContext context)
            : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Week>> GetByPlanIdAsync(Guid planId)
        {
            return await _context.Weeks
                .AsNoTracking()
                .Where(w => w.PlanId == planId)
                .OrderBy(w => w.WeekNumber)
                .ToListAsync();
        }
        public async Task<Week?> GetWeekWithModulesAsync(Guid weekId)
        {
            return await _context.Weeks
                .Include(w => w.Modules.OrderBy(m => m.OrderNo))
                    .ThenInclude(m => m.Lessons.OrderBy(l => l.OrderIndex))
                .Include(w => w.Modules)
                    .ThenInclude(m => m.Resources)
                .Include(w => w.Modules)
                    .ThenInclude(m => m.Assessments)
                        .ThenInclude(a => a.AssessmentQuestions)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.WeekId == weekId);
        }
        public async Task<bool> IsWeekUnlockedAsync(Guid weekId, Guid userId)
        {
            var week = await _context.Weeks
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.WeekId == weekId);

            if (week == null) return false;

            if (week.WeekNumber == 1) return true;

            var prevWeek = await _context.Weeks
                .AsNoTracking()
                .FirstOrDefaultAsync(w =>
                    w.PlanId == week.PlanId &&
                    w.WeekNumber == week.WeekNumber - 1);

            if (prevWeek == null) return true;

            var prevModules = await _context.Modules
                .Where(m => m.WeekId == prevWeek.WeekId)
                .ToListAsync();

            if (!prevModules.Any()) return true;

            foreach (var module in prevModules)
            {
                var progress = await _context.UserModuleProgresses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p =>
                        p.UserId == userId &&
                        p.ModuleId == module.ModuleId &&
                        p.Status == "Completed");

                if (progress == null) return false;
            }

            return true;
        }
        private async Task ClearLessonProgressForWeekAsync(Guid weekId)
        {
            var lessonIds = await _context.Lessons
                .Where(l => l.Module.WeekId == weekId)
                .Select(l => l.LessonId)
                .ToListAsync();

            if (!lessonIds.Any()) return;

            var progresses = await _context.UserModuleProgresses
                .Where(p => p.CurrentLessonId != null && lessonIds.Contains(p.CurrentLessonId.Value))
                .ToListAsync();

            foreach (var p in progresses)
                p.CurrentLessonId = null; 

            await _context.SaveChangesAsync();
        }
        public override async Task DeleteAsync(Week week)
        {
            if (week == null) return;
            await ClearLessonProgressForWeekAsync(week.WeekId);
            _context.Weeks.Remove(week);
            await _context.SaveChangesAsync();
        }
        public async Task<Week?> GetWeekFullByIdAsync(Guid weekId)
        {
            return await _context.Weeks
                .Include(w => w.Modules.OrderBy(m => m.OrderNo))
                    .ThenInclude(m => m.Lessons.OrderBy(l => l.OrderIndex))
                .Include(w => w.Modules)
                    .ThenInclude(m => m.Resources)
                .Include(w => w.Modules)
                    .ThenInclude(m => m.Assessments)
                        .ThenInclude(a => a.AssessmentQuestions)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.WeekId == weekId);
        }
        public async Task<IEnumerable<Week>> GetWeeksFullByPlanAsync(Guid planId)
        {
            return await _context.Weeks
                .Where(w => w.PlanId == planId)
                .OrderBy(w => w.WeekNumber)
                .Include(w => w.Modules.OrderBy(m => m.OrderNo))
                    .ThenInclude(m => m.Lessons.OrderBy(l => l.OrderIndex))
                .Include(w => w.Modules)
                    .ThenInclude(m => m.Resources)
                .Include(w => w.Modules)
                    .ThenInclude(m => m.Assessments)
                        .ThenInclude(a => a.AssessmentQuestions)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}

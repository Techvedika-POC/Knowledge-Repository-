using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class UserProgressRepository
        : GenericRepository<UserModuleProgress>, IUserProgressRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public UserProgressRepository(Knowledge_Repository_dbContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<bool> UserExistsAsync(Guid userId)
        {
            return await _context.Users.AnyAsync(u => u.UserId == userId);
        }

        public async Task<UserModuleProgress?> GetModuleProgressAsync(
           Guid userId,
           Guid weekId,
           Guid moduleId)
        {
            return await _dbSet.FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.ModuleId == moduleId);
        }

        public async Task InitializeModuleProgressAsync(
            Guid userId,
            Guid weekId,
            Guid moduleId)
        {
            var existing = await GetModuleProgressAsync(userId, weekId, moduleId);
            if (existing != null)
                return;

            int totalLessons = await _context.Lessons
                .CountAsync(l => l.ModuleId == moduleId);

            var progress = new UserModuleProgress
            {
                ProgressId = Guid.NewGuid(),
                UserId = userId,
                ModuleId = moduleId,
                Status = "InProgress",
                TestStatus = "NotTaken",
                TotalLessonsCount = totalLessons,   
                CompletedLessonsCount = 0,
                CompletedLessonIds = "[]",
                LessonProgressPercent = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await AddAsync(progress);
        }
        public async Task UpdateModuleProgressAsync(UserModuleProgress progress)
        {
            progress.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(progress);
        }
        public async Task<UserModuleProgress?> GetLessonProgressAsync(
            Guid userId,
            Guid lessonId)
        {
            return await _dbSet.FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.CurrentLessonId == lessonId);
        }
        public async Task TrackLessonAccessAsync(
            Guid userId,
            Guid moduleId,
            Guid lessonId)
        {
            var module = await _context.Modules
                .FirstAsync(m => m.ModuleId == moduleId);

            var progress = await GetModuleProgressAsync(
                userId,
                module.WeekId,
                moduleId);

            if (progress == null)
            {
                await InitializeModuleProgressAsync(userId, module.WeekId, moduleId);
                progress = await GetModuleProgressAsync(userId, module.WeekId, moduleId);
            }

            progress!.CurrentLessonId = lessonId;
            progress.LastAccessed = DateTime.UtcNow;
            progress.UpdatedAt = DateTime.UtcNow;

            await UpdateAsync(progress);
        }

        public async Task MarkLessonCompletedAsync(
            Guid userId,
            Guid moduleId,
            Guid lessonId)
        {
            var progress = await _dbSet.FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.ModuleId == moduleId);

            if (progress == null)
            {
                var module = await _context.Modules.FirstAsync(m => m.ModuleId == moduleId);

                progress = new UserModuleProgress
                {
                    ProgressId = Guid.NewGuid(),
                    UserId = userId,
                    ModuleId = moduleId,
                    TopicId = lessonId,              
                    CurrentLessonId = lessonId,      
                    TotalLessonsCount = await _context.Lessons.CountAsync(l => l.ModuleId == moduleId),
                    CompletedLessonsCount = 0,
                    CompletedLessonIds = "[]",
                    Status = "InProgress",
                    TestStatus = "NotTaken",
                    CreatedAt = DateTime.UtcNow
                };

                await _dbSet.AddAsync(progress);
            }

            var completed = JsonSerializer.Deserialize<List<string>>(progress.CompletedLessonIds ?? "[]");

            if (!completed.Contains(lessonId.ToString()))
            {
                completed.Add(lessonId.ToString());
                progress.CompletedLessonsCount++;
            }

            progress.CompletedLessonIds = JsonSerializer.Serialize(completed);
            progress.TopicId = lessonId;
            progress.CurrentLessonId = lessonId;
            progress.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task UpdateTestStatusAsync(
            Guid userId,
            Guid weekId,
            Guid moduleId,
            string testStatus)
        {
            var progress = await _dbSet.FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.ModuleId == moduleId);

            if (progress == null)
                throw new InvalidOperationException("Module progress must exist before test submission");

            testStatus = testStatus?.Trim();

            if (string.Equals(testStatus, "passed", StringComparison.OrdinalIgnoreCase))
                testStatus = "Passed";

            progress.TestStatus = testStatus;
            progress.TestAttemptedOn = DateTime.UtcNow;
            progress.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        public async Task<bool> TryMarkModuleCompletedAsync(
          Guid userId,
          Guid weekId,
          Guid moduleId)
        {
            var progress = await _dbSet.FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.ModuleId == moduleId);

            if (progress == null)
                return false;

            bool lessonsDone =
                progress.TotalLessonsCount == 0 ||
                progress.CompletedLessonsCount >= progress.TotalLessonsCount;

            bool testPassed =
                string.Equals(progress.TestStatus, "Passed", StringComparison.OrdinalIgnoreCase);

            if (!lessonsDone || !testPassed)
                return false;

            progress.Status = "Completed";
            progress.CompletedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> IsModuleUnlockedAsync(
            Guid userId,
            Guid weekId,
            Guid moduleId)
        {
            var module = await _context.Modules
                .FirstOrDefaultAsync(m => m.ModuleId == moduleId && m.WeekId == weekId);

            if (module == null)
                return false;

            if (module.OrderNo == 1)
                return true;

            var prevModule = await _context.Modules
                .FirstOrDefaultAsync(m =>
                    m.WeekId == weekId &&
                    m.OrderNo == module.OrderNo - 1);

            if (prevModule == null)
                return true;

            var prevProgress = await _dbSet.FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.ModuleId == prevModule.ModuleId);

            return prevProgress != null &&
                   prevProgress.Status == "Completed";
        }
        public async Task<bool> IsWeekUnlockedAsync(
            Guid userId,
            Guid planId,
            int weekNumber)
        {
            if (weekNumber == 1)
                return true;

            var prevWeek = await _context.Weeks
                .FirstOrDefaultAsync(w =>
                    w.PlanId == planId &&
                    w.WeekNumber == weekNumber - 1);

            if (prevWeek == null)
                return true;

            var modules = await _context.Modules
                .Where(m => m.WeekId == prevWeek.WeekId)
                .ToListAsync();

            foreach (var module in modules)
            {
                var progress = await _dbSet.FirstOrDefaultAsync(p =>
                    p.UserId == userId &&
                    p.ModuleId == module.ModuleId);

                if (progress == null || progress.Status != "Completed")
                    return false;
            }

            return true;
        }
    }
}

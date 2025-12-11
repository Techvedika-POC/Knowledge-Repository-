using Application.Interfaces;
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
    public class UserProgressRepository : GenericRepository<UserModuleProgress>, IUserProgressRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public UserProgressRepository(Knowledge_Repository_dbContext context)
            : base(context)
        {
            _context = context;
        }

        // --------------------------------------------------------------------
        // GET MODULE PROGRESS
        // --------------------------------------------------------------------
        public async Task<UserModuleProgress?> GetModuleProgressAsync(Guid userId, Guid weekId, Guid moduleId)
        {
            return await _dbSet.FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.ModuleId == moduleId);
        }

        // --------------------------------------------------------------------
        // GET LESSON PROGRESS  ✅ (NOW INCLUDED)
        // --------------------------------------------------------------------
        public async Task<UserModuleProgress?> GetLessonProgressAsync(Guid userId, Guid lessonId)
        {
            return await _dbSet.FirstOrDefaultAsync(p =>
                p.UserId == userId &&
                p.CurrentLessonId == lessonId);
        }

        // --------------------------------------------------------------------
        // INITIALIZE MODULE PROGRESS
        // --------------------------------------------------------------------
        public async Task InitializeModuleProgressAsync(Guid userId, Guid weekId, Guid moduleId)
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
                TopicId = Guid.Empty,
                Status = "NotStarted",
                TestStatus = "NotTaken",
                TotalLessonsCount = totalLessons,
                CompletedLessonsCount = 0,
                CompletedLessonIds = "[]",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await AddAsync(progress);
        }

        // --------------------------------------------------------------------
        // UPDATE MODULE PROGRESS   ✅ (NOW INCLUDED)
        // --------------------------------------------------------------------
        public async Task UpdateModuleProgressAsync(UserModuleProgress progress)
        {
            progress.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(progress);
        }

        // --------------------------------------------------------------------
        // TRACK LESSON ACCESS
        // --------------------------------------------------------------------
        public async Task TrackLessonAccessAsync(Guid userId, Guid moduleId, Guid lessonId)
        {
            var progress = await _dbSet
                .FirstOrDefaultAsync(p => p.UserId == userId && p.ModuleId == moduleId);

            if (progress == null)
                return;

            progress.CurrentLessonId = lessonId;
            progress.LastAccessed = DateTime.UtcNow;
            progress.UpdatedAt = DateTime.UtcNow;

            await UpdateAsync(progress);
        }

        // --------------------------------------------------------------------
        // MARK LESSON COMPLETED
        // --------------------------------------------------------------------
        public async Task MarkLessonCompletedAsync(Guid userId, Guid moduleId, Guid lessonId)
        {
            var progress = await _dbSet
                .FirstOrDefaultAsync(p => p.UserId == userId && p.ModuleId == moduleId);

            if (progress == null)
                return;

            var completedIds = JsonSerializer.Deserialize<string[]>(progress.CompletedLessonIds ?? "[]")!
                .ToList();

            if (!completedIds.Contains(lessonId.ToString()))
                completedIds.Add(lessonId.ToString());

            progress.CompletedLessonIds = JsonSerializer.Serialize(completedIds);
            progress.CompletedLessonsCount = completedIds.Count;

            if (progress.TotalLessonsCount > 0)
            {
                progress.LessonProgressPercent =
                    Math.Round((decimal)progress.CompletedLessonsCount / progress.TotalLessonsCount * 100, 2);
            }

            progress.UpdatedAt = DateTime.UtcNow;

            await UpdateAsync(progress);
        }

        // --------------------------------------------------------------------
        // UPDATE TEST STATUS
        // --------------------------------------------------------------------
        public async Task UpdateTestStatusAsync(Guid userId, Guid weekId, Guid moduleId, string testStatus)
        {
            var progress = await GetModuleProgressAsync(userId, weekId, moduleId);

            if (progress == null)
            {
                await InitializeModuleProgressAsync(userId, weekId, moduleId);
                progress = await GetModuleProgressAsync(userId, weekId, moduleId);
            }

            progress.TestStatus = testStatus;
            progress.TestAttemptedOn = DateTime.UtcNow;
            progress.UpdatedAt = DateTime.UtcNow;

            bool lessonsDone = progress.CompletedLessonsCount == progress.TotalLessonsCount;
            bool passedTest = testStatus.Equals("Passed", StringComparison.OrdinalIgnoreCase);

            if (lessonsDone && passedTest)
            {
                progress.Status = "Completed";
                progress.CompletedOn = DateTime.UtcNow;
            }

            await UpdateAsync(progress);
        }

        // --------------------------------------------------------------------
        // TRY TO MARK MODULE COMPLETED
        // --------------------------------------------------------------------
        public async Task<bool> TryMarkModuleCompletedAsync(Guid userId, Guid weekId, Guid moduleId)
        {
            var progress = await GetModuleProgressAsync(userId, weekId, moduleId);
            if (progress == null)
                return false;

            bool lessonsDone = progress.CompletedLessonsCount == progress.TotalLessonsCount;
            bool passedTest = progress.TestStatus == "Passed";

            if (!lessonsDone || !passedTest)
                return false;

            progress.Status = "Completed";
            progress.CompletedOn = DateTime.UtcNow;
            progress.UpdatedAt = DateTime.UtcNow;

            await UpdateAsync(progress);
            return true;
        }

        // --------------------------------------------------------------------
        // MODULE UNLOCK LOGIC
        // --------------------------------------------------------------------
        public async Task<bool> IsModuleUnlockedAsync(Guid userId, Guid weekId, Guid moduleId)
        {
            var module = await _context.Modules.FirstOrDefaultAsync(m => m.ModuleId == moduleId);
            if (module == null) return false;

            if (module.OrderNo == 1)
                return true;

            var prevModule = await _context.Modules.FirstOrDefaultAsync(m =>
                m.WeekId == module.WeekId &&
                m.OrderNo == module.OrderNo - 1);

            if (prevModule == null)
                return true;

            var previousProgress =
                await GetModuleProgressAsync(userId, weekId, prevModule.ModuleId);

            return previousProgress != null &&
                   previousProgress.Status == "Completed";
        }
        public async Task<bool> UserExistsAsync(Guid userId)
        {
            return await _context.Users.AnyAsync(u => u.UserId == userId);
        }

        // --------------------------------------------------------------------
        // WEEK UNLOCK LOGIC
        // --------------------------------------------------------------------
        public async Task<bool> IsWeekUnlockedAsync(Guid userId, Guid planId, int weekNumber)
        {
            if (weekNumber == 1)
                return true;

            var prevWeek = await _context.Weeks
                .FirstOrDefaultAsync(w => w.PlanId == planId && w.WeekNumber == weekNumber - 1);

            if (prevWeek == null)
                return true;

            var modules = await _context.Modules
                .Where(m => m.WeekId == prevWeek.WeekId)
                .ToListAsync();

            foreach (var module in modules)
            {
                var progress =
                    await GetModuleProgressAsync(userId, prevWeek.WeekId, module.ModuleId);

                if (progress == null || progress.Status != "Completed")
                    return false;
            }

            return true;
        }
    }
}

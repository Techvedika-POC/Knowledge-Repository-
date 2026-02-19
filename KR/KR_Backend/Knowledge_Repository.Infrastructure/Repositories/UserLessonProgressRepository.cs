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
    public class UserLessonProgressRepository
        : IUserLessonProgressRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public UserLessonProgressRepository(
            Knowledge_Repository_dbContext context)
        {
            _context = context;
        }
        public async Task<UserLessonProgress?> GetAsync(
            Guid userId, Guid lessonId)
        {
            return await _context.UserLessonProgresses
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.LessonId == lessonId);
        }

        public async Task StartAsync(
            Guid userId, Guid lessonId, Guid moduleId)
        {
            var progress = await GetAsync(userId, lessonId);

            if (progress == null)
            {
                progress = new UserLessonProgress
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    LessonId = lessonId,
                    ModuleId = moduleId,
                    Status = "InProgress",
                    StartedOn = DateTime.UtcNow,
                    LastAccessed = DateTime.UtcNow
                };

                _context.UserLessonProgresses.Add(progress);
            }
            else
            {
                progress.Status = "InProgress";
                progress.LastAccessed = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task CompleteAsync(
            Guid userId, Guid lessonId)
        {
            var progress = await GetAsync(userId, lessonId);

            if (progress == null)
                throw new InvalidOperationException(
                    "Lesson must be started before completion.");

            progress.Status = "Completed";
            progress.CompletedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        public async Task<List<UserLessonProgress>> GetByModuleAsync(
            Guid userId, Guid moduleId)
        {
            return await _context.UserLessonProgresses
                .Where(x =>
                    x.UserId == userId &&
                    x.ModuleId == moduleId)
                .ToListAsync();
        }
    }
}

using Knowledge_Repository.Application.Dtos;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IUserProgressService
    {
        Task<UserProgressDto> GetUserProgressAsync(Guid userId, Guid planId);

        // MODULE COMPLETION (only allowed if: all lessons completed + test passed)
        Task<bool> TryMarkModuleCompletedAsync(Guid userId, Guid weekId, Guid moduleId);

        // UPDATE TEST STATUS (Pass/Fail)
        Task UpdateTestStatusAsync(Guid userId, Guid weekId, Guid moduleId, string testStatus);

        // LESSON PROGRESS
        Task TrackLessonAccessAsync(Guid userId, Guid moduleId, Guid lessonId);
        Task MarkLessonCompletedAsync(Guid userId, Guid moduleId, Guid lessonId);
    }
}

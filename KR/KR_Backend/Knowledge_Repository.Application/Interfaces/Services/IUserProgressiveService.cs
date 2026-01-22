using Knowledge_Repository.Application.Dtos;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IUserProgressService
    {
        Task<UserProgressDto> GetUserProgressAsync(Guid userId, Guid planId);
        Task<bool> TryMarkModuleCompletedAsync(Guid userId, Guid weekId, Guid moduleId);
        Task UpdateTestStatusAsync(Guid userId, Guid weekId, Guid moduleId, string testStatus);
        Task TrackLessonAccessAsync(Guid userId, Guid moduleId, Guid lessonId);
        Task MarkLessonCompletedAsync(Guid userId, Guid moduleId, Guid lessonId);
    }
}

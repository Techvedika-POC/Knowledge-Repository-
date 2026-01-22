using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;

public interface IUserProgressRepository : IGenericRepository<UserModuleProgress>
{
    Task<bool> UserExistsAsync(Guid userId);

    Task<UserModuleProgress?> GetModuleProgressAsync(Guid userId, Guid weekId, Guid moduleId);
    Task InitializeModuleProgressAsync(Guid userId, Guid weekId, Guid moduleId);
    Task UpdateModuleProgressAsync(UserModuleProgress progress);

    Task<UserModuleProgress?> GetLessonProgressAsync(Guid userId, Guid lessonId);
    Task TrackLessonAccessAsync(Guid userId, Guid moduleId, Guid lessonId);
    Task MarkLessonCompletedAsync(Guid userId, Guid moduleId, Guid lessonId);

    Task UpdateTestStatusAsync(Guid userId, Guid weekId, Guid moduleId, string testStatus);

    Task<bool> TryMarkModuleCompletedAsync(Guid userId, Guid weekId, Guid moduleId);

    Task<bool> IsModuleUnlockedAsync(Guid userId, Guid weekId, Guid moduleId);
    Task<bool> IsWeekUnlockedAsync(Guid userId, Guid planId, int weekNumber);

}

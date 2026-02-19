using Knowledge_Repository.Application.Dtos;

public interface ILearningPlanService
{
    Task<List<LearningPlanDto>> GetAllPlansAsync();

    Task<LearningPlanDto?> GetPlanHierarchyAsync(Guid planId);
    Task<LearningPlanFullDto?> GetPlanHierarchyFullAsync(Guid planId, Guid? userId = null);

    Task<bool> IsPlanCompletedAsync(Guid planId, Guid userId);

    Task<LearningPlanDto> GenerateLearningPlanAsync(string title, bool useAI);

    Task<LearningPlanFullDto> CreateFullLearningPlanAsync(LearningPlanFullDto dto);
    Task<bool> UpdateLearningPlanAsync(LearningPlanFullDto dto);
    Task<bool> DeleteLearningPlanAsync(Guid planId);

    Task<bool> UpdateWeekAsync(WeekFullDto dto);
    Task<bool> UpdateModuleAsync(ModuleDetailDto dto);
    Task<bool> UpdateLessonAsync(LessonDto dto);

    Task<bool> DeleteModuleAsync(Guid moduleId);
}

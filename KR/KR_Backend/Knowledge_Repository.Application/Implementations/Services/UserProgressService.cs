using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;

public class UserProgressService : IUserProgressService
{
    private readonly IUserLessonProgressRepository _lessonRepo;
    private readonly IUserAssessmentProgressRepository _assessmentRepo;
    private readonly IUserPlanEnrollmentRepository _enrollmentRepo;
    private readonly ILearningEventRepository _eventRepo;
    private readonly IModuleRepository _moduleRepo;
    private readonly IUserModuleProgressRepository _moduleProgressRepo;

    public UserProgressService(
        IUserLessonProgressRepository lessonRepo,
        IUserAssessmentProgressRepository assessmentRepo,
        IUserPlanEnrollmentRepository enrollmentRepo,
        ILearningEventRepository eventRepo,
        IModuleRepository moduleRepo,
        IUserModuleProgressRepository moduleProgressRepo)
    {
        _lessonRepo = lessonRepo;
        _assessmentRepo = assessmentRepo;
        _enrollmentRepo = enrollmentRepo;
        _eventRepo = eventRepo;
        _moduleRepo = moduleRepo;
        _moduleProgressRepo = moduleProgressRepo;
    }

    public async Task EnrollUserToPlanAsync(
        Guid userId, Guid planId, Guid assignedBy)
    {
        await _enrollmentRepo.EnrollAsync(userId, planId, assignedBy);

        await _eventRepo.LogAsync(
            userId,
            "PLAN_ASSIGNED",
            "PLAN",
            planId);
    }

    public async Task StartPlanAsync(Guid userId, Guid planId)
    {
        await _enrollmentRepo.StartAsync(userId, planId);

        await _eventRepo.LogAsync(
            userId,
            "PLAN_STARTED",
            "PLAN",
            planId);
    }

    public async Task StartLessonAsync(
        Guid userId, Guid lessonId, Guid moduleId)
    {
        await _lessonRepo.StartAsync(userId, lessonId, moduleId);
        await _moduleProgressRepo.TouchAsync(userId, moduleId);

        await _eventRepo.LogAsync(
            userId,
            "LESSON_STARTED",
            "LESSON",
            lessonId);
    }

    public async Task CompleteLessonAsync(
        Guid userId, Guid lessonId)
    {
        await _lessonRepo.CompleteAsync(userId, lessonId);

        var lesson = await _lessonRepo.GetAsync(userId, lessonId);
        if (lesson == null) return;
        await RecalculateModuleProgress(userId, lesson.ModuleId);

        await _eventRepo.LogAsync(
            userId,
            "LESSON_COMPLETED",
            "LESSON",
            lessonId);
    }

    public async Task StartAssessmentAsync(
        Guid userId, Guid assessmentId, Guid moduleId)
    {
        await _assessmentRepo.StartAsync(
            userId, assessmentId, moduleId);

        await _moduleProgressRepo.TouchAsync(userId, moduleId);

        await _eventRepo.LogAsync(
            userId,
            "ASSESSMENT_STARTED",
            "ASSESSMENT",
            assessmentId);
    }
    public async Task<AssessmentResultDto> SubmitAssessmentAsync(
        SubmitAssessmentDto dto)
    {
        var progress =
            await _assessmentRepo.GetAsync(dto.UserId, dto.AssessmentId);

        if (progress == null)
            throw new Exception("Assessment not started");
        await RecalculateModuleProgress(dto.UserId, progress.ModuleId);

        return new AssessmentResultDto
        {
            AssessmentId = dto.AssessmentId,
            UserId = dto.UserId,
            Passed = progress.Passed ?? false,
            ScorePercentage = (double)(progress.Score ?? 0),
            IsCompleted = progress.Status == "Passed",
            AttemptedOn = progress.CompletedOn ?? DateTime.UtcNow
        };
    }


    private async Task RecalculateModuleProgress(Guid userId, Guid moduleId)
    {
        var lessons =
            await _lessonRepo.GetByModuleAsync(userId, moduleId);

        var assessments =
            await _assessmentRepo.GetByModuleAsync(userId, moduleId);

        int total = lessons.Count + assessments.Count;

        int completed =
            lessons.Count(l => l.Status == "Completed") +
            assessments.Count(a => a.Passed == true);

        if (total == 0) return;

        await _moduleProgressRepo.TouchAsync(userId, moduleId);

        if (completed == total)
        {
            await _moduleProgressRepo.CompleteAsync(userId, moduleId);
            await UnlockNextModule(userId, moduleId);
        }
    }



    private async Task UnlockNextModule(Guid userId, Guid moduleId)
    {
        var module = await _moduleRepo.GetByIdAsync(moduleId);
        if (module == null) return;

        var modules =
            await _moduleRepo.GetByWeekIdAsync(module.WeekId);

        var ordered = modules
            .OrderBy(m => m.OrderNo)
            .ToList();

        var index = ordered.FindIndex(m => m.ModuleId == moduleId);
        if (index == -1 || index == ordered.Count - 1) return;

        var next = ordered[index + 1];

        await _moduleProgressRepo.TouchAsync(
            userId,
            next.ModuleId);
    }

    public async Task<decimal> GetModuleProgressAsync(
        Guid userId, Guid moduleId)
    {
        var lessons =
            await _lessonRepo.GetByModuleAsync(userId, moduleId);

        var assessments =
            await _assessmentRepo.GetByModuleAsync(userId, moduleId);

        int total =
            lessons.Count + assessments.Count;

        if (total == 0) return 0;

        int completed =
            lessons.Count(l => l.Status == "Completed") +
            assessments.Count(a => a.Passed == true);

        return (decimal)completed / total * 100;
    }

    public async Task<decimal> GetPlanProgressAsync(
        Guid userId, Guid planId)
    {
        var modules =
            await _moduleRepo.GetModulesByPlanIdAsync(planId);

        if (!modules.Any()) return 0;

        decimal sum = 0;
        foreach (var m in modules)
            sum += await GetModuleProgressAsync(
                userId, m.ModuleId);

        return sum / modules.Count();
    }
}

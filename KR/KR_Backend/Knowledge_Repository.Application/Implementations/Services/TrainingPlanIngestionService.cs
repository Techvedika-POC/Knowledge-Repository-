using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

public class TrainingPlanIngestionService : ITrainingPlanIngestionService
{
    private readonly ITrainingPlanRepository _trainingPlanRepository;
    private readonly ITrainingPlanMappingService _mappingService;
    private readonly ILogger<TrainingPlanIngestionService> _logger;

    public TrainingPlanIngestionService(
        ITrainingPlanRepository trainingPlanRepository,
        ITrainingPlanMappingService mappingService,
        ILogger<TrainingPlanIngestionService> logger)
    {
        _trainingPlanRepository = trainingPlanRepository;
        _mappingService = mappingService;
        _logger = logger;
    }

    public async Task<Guid> IngestTrainingPlanAsync(
        TrainingPlanIngestionDto trainingPlanDto,
        Guid userId)
    {
        if (trainingPlanDto == null)
            throw new ArgumentNullException(nameof(trainingPlanDto));

        if (trainingPlanDto.Weeks == null || trainingPlanDto.Weeks.Count == 0)
            throw new InvalidOperationException("Training plan contains no weeks.");

        _logger.LogInformation("Mapping training plan for user {UserId}", userId);

        var learningPlan =
            _mappingService.MapToLearningPlan(trainingPlanDto, userId);

        _logger.LogInformation("Persisting training plan {PlanId}", learningPlan.PlanId);

        await _trainingPlanRepository.AddAsync(learningPlan);

        _logger.LogInformation("Training plan {PlanId} saved successfully", learningPlan.PlanId);

        return learningPlan.PlanId;
    }
}

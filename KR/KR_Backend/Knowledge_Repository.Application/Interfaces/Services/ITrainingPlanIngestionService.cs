using Knowledge_Repository.Application.Dtos;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface ITrainingPlanIngestionService
    {
        Task<Guid> IngestTrainingPlanAsync(
            TrainingPlanIngestionDto trainingPlanDto,
            Guid userId
        );
    }
}

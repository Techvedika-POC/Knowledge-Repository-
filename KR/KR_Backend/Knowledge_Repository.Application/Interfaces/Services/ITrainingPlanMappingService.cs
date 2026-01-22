using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Domain.Entities;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface ITrainingPlanMappingService
    {
        LearningPlan MapToLearningPlan(
            TrainingPlanIngestionDto dto,
            Guid createdBy
        );
    }
}

using Knowledge_Repository.Domain.Entities;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface ITrainingPlanRepository
    {
        Task AddAsync(LearningPlan plan);
        Task<LearningPlan?> GetByIdAsync(Guid planId);
        Task UpdateAsync(LearningPlan plan);
        Task DeleteAsync(Guid planId);
    }

}

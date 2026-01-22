using Knowledge_Repository.Application.Dtos;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IManagerRepository
    {
        Task AssignLearningPlanAsync(Guid planId, Guid managerId, List<Guid> userIds);

        Task<List<UserLearningProgressDto>> GetPlanProgressAsync(Guid planId);

        Task<List<LearningPlanDto>> GetAllLearningPlansAsync();
    }
}

using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class ManagerService : IManagerService
    {
        private readonly IManagerRepository _repo;

        public ManagerService(IManagerRepository repo)
        {
            _repo = repo;
        }

        public async Task AssignLearningPlanAsync(Guid planId, Guid managerId, List<Guid> userIds)
        {
            if (userIds == null || !userIds.Any())
                throw new ArgumentException("No users selected");

            await _repo.AssignLearningPlanAsync(planId, managerId, userIds);
        }

        public async Task<List<UserLearningProgressDto>> GetPlanProgressAsync(Guid planId)
        {
            return await _repo.GetPlanProgressAsync(planId);
        }

        public async Task<List<LearningPlanDto>> GetAllLearningPlansAsync()
        {
            return await _repo.GetAllLearningPlansAsync();
        }
    }
}

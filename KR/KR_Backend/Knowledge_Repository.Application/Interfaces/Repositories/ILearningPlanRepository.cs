
using Knowledge_Repository.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface ILearningPlanRepository : IGenericRepository<LearningPlan>
    {
        Task<LearningPlan?> GetPlanWithHierarchyAsync(Guid planId);
        Task<LearningPlan?> GetPlanWithHierarchyFullAsync(Guid planId);
        Task<bool> IsPlanCompletedByUser(Guid planId, Guid userId);
    }
}

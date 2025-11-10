using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IVLearnModuleRepository : IGenericRepository<Module>
    {
        Task<IEnumerable<Module>> GetModulesByTopicAndUserAsync(Guid topicId, Guid userId);
        Task<bool> UpdateTestStatusAsync(Guid moduleId, Guid userId, bool isCompleted);
    }
}

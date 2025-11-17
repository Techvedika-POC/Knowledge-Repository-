using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IVLearnModuleRepository : IGenericRepository<Module>
    {
        Task<Module> AddModuleAsync(Module module);
        Task<bool> ModuleNameExistsInTopicAsync(Guid topicId, string moduleName);
        Task<IEnumerable<Module>> GetModulesByTopicAsync(Guid topicId);
        Task<bool> UpdateTestStatusAsync(Guid moduleId, Guid userId, bool isCompleted);
    }
}

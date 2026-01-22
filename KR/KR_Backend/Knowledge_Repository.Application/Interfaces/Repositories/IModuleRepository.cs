using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IModuleRepository : IGenericRepository<Module>
    {
        Task<IEnumerable<Module>> GetByWeekIdAsync(Guid weekId);
        Task<IEnumerable<Module>> GetModulesByPlanIdAsync(Guid planId);

        Task<Module?> GetModuleFullAsync(Guid moduleId);
        Task<bool> IsModuleUnlockedAsync(Guid moduleId, Guid userId);
    }
}

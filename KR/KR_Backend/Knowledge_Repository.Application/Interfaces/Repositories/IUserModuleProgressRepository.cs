using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IUserModuleProgressRepository
    {
        Task<UserModuleProgress?> GetAsync(Guid userId, Guid moduleId);
        Task<IEnumerable<UserModuleProgress>> GetByWeekAsync(Guid userId, Guid weekId);
        Task TouchAsync(Guid userId, Guid moduleId);
        Task CompleteAsync(Guid userId, Guid moduleId);
    }


}

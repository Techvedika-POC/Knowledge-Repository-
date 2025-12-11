using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IResourceRepository : IGenericRepository<Resource>
    {
        Task<IEnumerable<Resource>> GetByTopicOrModuleAsync(Guid topicId, Guid? moduleId);
        Task AddBatchAsync(IEnumerable<Resource> resources);
    }
}

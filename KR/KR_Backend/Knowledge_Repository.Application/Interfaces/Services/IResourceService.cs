using Knowledge_Repository.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Services
{
    public interface IResourceService
    {
        Task<ResourceDto> GetResourceByIdAsync(Guid resourceId);
        Task<IEnumerable<ResourceDto>> GetResourcesByTopicOrModuleAsync(Guid topicId, Guid? moduleId = null, int page = 1, int pageSize = 20);
        Task<ResourceDto> CreateResourceAsync(ResourceDto resourceDto);
        Task<IEnumerable<ResourceDto>> CreateResourcesBatchAsync(IEnumerable<ResourceDto> resources);
        Task UpdateResourceAsync(ResourceDto resourceDto);
        Task UpdateResourcesBatchAsync(IEnumerable<ResourceDto> resources);
        Task DeleteResourceAsync(Guid resourceId);
        Task MarkResourceAccessedAsync(Guid resourceId, Guid userId);
    }
}

using Application.Interfaces;
using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class ResourceService : IResourceService
    {
        private readonly IResourceRepository _resourceRepo;
        private readonly IUserProgressRepository _userProgressRepo;

        public ResourceService(IResourceRepository resourceRepo, IUserProgressRepository userProgressRepo)
        {
            _resourceRepo = resourceRepo;
            _userProgressRepo = userProgressRepo;
        }

        private ResourceDto MapToDto(Resource r) => new ResourceDto
        {
            ResourceId = r.ResourceId,
            Title = r.Title ?? string.Empty,
            Url = r.Url ?? string.Empty,
            ResourceType = r.ResourceType ?? string.Empty,
            ModuleId = r.ModuleId,
            TopicId = r.TopicId,
            Description = r.Description ?? string.Empty,
            IsAiGenerated = r.IsAiGenerated ?? false,
            Metadata = string.IsNullOrEmpty(r.Metadata) ? "{}" : r.Metadata,
            CreatedOn = r.CreatedOn,
            UpdatedOn = r.UpdatedOn
        };

        private string SafeJson(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "{}";
            try
            {
                JsonDocument.Parse(input);
                return input;
            }
            catch
            {
                return JsonSerializer.Serialize(input);
            }
        }

        public async Task<ResourceDto> GetResourceByIdAsync(Guid resourceId)
        {
            var resource = await _resourceRepo.GetByIdAsync(resourceId);
            return resource == null ? null : MapToDto(resource);
        }

        public async Task<IEnumerable<ResourceDto>> GetResourcesByTopicOrModuleAsync(Guid topicId, Guid? moduleId = null, int page = 1, int pageSize = 20)
        {
            var resources = await _resourceRepo.GetByTopicOrModuleAsync(topicId, moduleId);

            return resources
                .OrderBy(r => r.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToDto);
        }

        public async Task<ResourceDto> CreateResourceAsync(ResourceDto resourceDto)
        {
            var entity = new Resource
            {
                ResourceId = Guid.NewGuid(),
                Title = resourceDto.Title,
                Url = resourceDto.Url,
                ResourceType = resourceDto.ResourceType,
                ModuleId = resourceDto.ModuleId,
                TopicId = resourceDto.TopicId,
                Description = resourceDto.Description,
                Metadata = SafeJson(resourceDto.Metadata),
                IsAiGenerated = resourceDto.IsAiGenerated,
                CreatedOn = DateTime.UtcNow
            };

            await _resourceRepo.AddAsync(entity);
            return MapToDto(entity);
        }

        public async Task<IEnumerable<ResourceDto>> CreateResourcesBatchAsync(IEnumerable<ResourceDto> resources)
        {
            var entities = resources.Select(r => new Resource
            {
                ResourceId = Guid.NewGuid(),
                Title = r.Title,
                Url = r.Url,
                ResourceType = r.ResourceType,
                ModuleId = r.ModuleId,
                TopicId = r.TopicId,
                Description = r.Description,
                Metadata = SafeJson(r.Metadata),
                IsAiGenerated = r.IsAiGenerated,
                CreatedOn = DateTime.UtcNow 
            }).ToList();

            await _resourceRepo.AddBatchAsync(entities);
            return entities.Select(MapToDto);
        }

        public async Task UpdateResourceAsync(ResourceDto resourceDto)
        {
            var resource = await _resourceRepo.GetByIdAsync(resourceDto.ResourceId);
            if (resource == null) return;

            resource.Title = resourceDto.Title;
            resource.Url = resourceDto.Url;
            resource.ResourceType = resourceDto.ResourceType;
            resource.ModuleId = resourceDto.ModuleId;
            resource.TopicId = resourceDto.TopicId;
            resource.Description = resourceDto.Description;
            resource.Metadata = SafeJson(resourceDto.Metadata);
            resource.IsAiGenerated = resourceDto.IsAiGenerated;
            resource.UpdatedOn = DateTime.Now; 

            await _resourceRepo.UpdateAsync(resource);
        }

        public async Task UpdateResourcesBatchAsync(IEnumerable<ResourceDto> resources)
        {
            foreach (var r in resources)
            {
                if (r.ResourceId != Guid.Empty)
                {
                    var resource = await _resourceRepo.GetByIdAsync(r.ResourceId);
                    if (resource != null)
                    {
                        resource.Title = r.Title;
                        resource.Url = r.Url;
                        resource.ResourceType = r.ResourceType;
                        resource.ModuleId = r.ModuleId;
                        resource.TopicId = r.TopicId;
                        resource.Description = r.Description;
                        resource.Metadata = SafeJson(r.Metadata);
                        resource.IsAiGenerated = r.IsAiGenerated;
                        resource.UpdatedOn = DateTime.Now;

                        await _resourceRepo.UpdateAsync(resource);
                        continue;
                    }
                }

                var newResource = new Resource
                {
                    ResourceId = Guid.NewGuid(),
                    Title = r.Title,
                    Url = r.Url,
                    ResourceType = r.ResourceType,
                    ModuleId = r.ModuleId,
                    TopicId = r.TopicId,
                    Description = r.Description,
                    Metadata = SafeJson(r.Metadata),
                    IsAiGenerated = r.IsAiGenerated,
                    CreatedOn = DateTime.Now
                };

                await _resourceRepo.AddAsync(newResource);
            }
        }

        public async Task DeleteResourceAsync(Guid resourceId)
        {
            var resource = await _resourceRepo.GetByIdAsync(resourceId);
            if (resource == null) return;

            await _resourceRepo.DeleteAsync(resource);
        }

        public async Task MarkResourceAccessedAsync(Guid resourceId, Guid userId)
        {
            var resource = await _resourceRepo.GetByIdAsync(resourceId);
            if (resource == null) return;

            await _userProgressRepo.InitializeModuleProgressAsync(
                userId,
                resource.TopicId,
                resource.ModuleId ?? Guid.Empty
            );

            var progress = await _userProgressRepo.GetModuleProgressAsync(
                userId,
                resource.TopicId,
                resource.ModuleId ?? Guid.Empty
            );

            if (progress != null)
            {
                progress.LastAccessed = DateTime.UtcNow; 
                await _userProgressRepo.UpdateModuleProgressAsync(progress);
            }
        }
    }
}

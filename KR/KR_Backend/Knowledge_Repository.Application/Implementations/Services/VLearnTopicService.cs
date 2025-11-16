using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class VLearnTopicService : IVLearnTopicService
    {
        private readonly IVLearnTopicRepository _topicRepository;
        private readonly IVLearnModuleRepository _moduleRepository;

        public VLearnTopicService(IVLearnTopicRepository topicRepository, IVLearnModuleRepository moduleRepository)
        {
            _topicRepository = topicRepository ?? throw new ArgumentNullException(nameof(topicRepository));
            _moduleRepository = moduleRepository ?? throw new ArgumentNullException(nameof(moduleRepository));
        }

        public async Task<IEnumerable<Topic>> GetAllTopicsAsync()
        {
            return await _topicRepository.GetAllTopicsAsync();
        }

        public async Task<VLearnTopicDto> GetTopicByIdAsync(Guid topicId)
        {
            var topic = await _topicRepository.GetByIdAsync(topicId);
            if (topic == null) return null;

            var modules = await _moduleRepository.GetModulesByTopicAsync(topicId);

            var dto = new VLearnTopicDto
            {
                TopicId = topic.TopicId,
                TopicName = topic.TopicName,
                Description = topic.Description,
                CreatedOn = topic.CreatedOn,
                UpdatedOn = topic.UpdatedOn,
                CreatedBy = topic.CreatedBy,
                UpdatedBy = topic.UpdatedBy,
                CreatedByNavigation = topic.CreatedByNavigation,
                UpdatedByNavigation = topic.UpdatedByNavigation,
                Modules = modules?.ToList() ?? new List<Module>()
            };

            return dto;
        }

        public async Task<VLearnTopicDto> AddTopicAsync(CreateTopicDto dto, Guid createdBy)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.TopicName)) throw new ArgumentException("TopicName is required", nameof(dto.TopicName));

            var exists = await _topicRepository.TopicNameExistsAsync(dto.TopicName);
            if (exists) throw new InvalidOperationException("A topic with the same name already exists.");

            var topic = new Topic
            {
                TopicId = Guid.NewGuid(),
                TopicName = dto.TopicName.Trim(),
                Description = dto.Description,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = createdBy == Guid.Empty ? (Guid?)null : createdBy
            };

            await _topicRepository.AddTopicAsync(topic);

            var result = new VLearnTopicDto
            {
                TopicId = topic.TopicId,
                TopicName = topic.TopicName,
                Description = topic.Description,
                CreatedOn = topic.CreatedOn,
                CreatedBy = topic.CreatedBy,
                Modules = new List<Module>()
            };

            return result;
        }
        public async Task<(IEnumerable<VLearnTopicDto> Items, int Total)> SearchTopicsAsync(string q, int page, int size)
        {
            
            if (page < 1) page = 1;
            if (size < 1) size = 10;
            if (size > 100) size = 100; 

            \
            var query = _topicRepository.Query();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var trimmed = q.Trim();
                var pattern = $"%{trimmed}%";

                query = query.Where(t =>
                    EF.Functions.ILike(t.TopicName, pattern) ||
                    EF.Functions.ILike(t.Description ?? string.Empty, pattern)
                );
            }

         
            var total = await query.CountAsync();

           
            var topics = await query
                .OrderBy(t => t.TopicName)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            var items = topics.Select(t => new VLearnTopicDto
            {
                TopicId = t.TopicId,
                TopicName = t.TopicName,
                Description = t.Description,
                CreatedOn = t.CreatedOn,
                UpdatedOn = t.UpdatedOn,
                CreatedBy = t.CreatedBy,
                UpdatedBy = t.UpdatedBy,
              
                Modules = new List<Module>()
            }).ToList();

            return (items, total);
        }
    }
}

using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Knowledge_Repository.Application.Implementations.Services
{
    public class FreshPickService : IFreshPickService
    {
        private readonly IKnowledgeItemRepository _knowledgeItemRepository;

        public FreshPickService(IKnowledgeItemRepository knowledgeItemRepository)
        {
            _knowledgeItemRepository = knowledgeItemRepository;
        }

        public async Task<List<KnowledgeItemDto>> GetFreshPicksAsync(int count = 10)
        {
            var items = await _knowledgeItemRepository.GetFreshPicksAsync(count);

            var freshPicks = items.Select(k => new KnowledgeItemDto
            {
                ItemId = k.ItemId,
                Title = k.Title,
                Description = k.Description,
                DomainId = k.DomainId,
                DomainName = k.Domain?.DomainName ?? "Unknown",
                CategoryId = k.CategoryId,
                CategoryName = k.Category?.CategoryName ?? "Unknown",
                Language = k.Language,
                Framework = k.Framework,
                OwnerId = k.OwnerId,
                OwnerName = k.Owner?.Name ?? "Unknown",
                CreatedOn = k.CreatedOn ?? DateTime.MinValue,
                Tags = k.KnowledgeTags?.Select(t => t.TagName).ToList() ?? new List<string>(),
                EngagementScore = k.Engagements?.Count ?? 0
            }).ToList();

            return freshPicks;
        }

    }
}

using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class GlobalSearchService : IGlobalSearchService
    {
        private readonly IKnowledgeItemRepository _knowledgeItemRepository;

        public GlobalSearchService(IKnowledgeItemRepository knowledgeItemRepository)
        {
            _knowledgeItemRepository = knowledgeItemRepository;
        }

        public async Task<List<KnowledgeItemDto>> GlobalSearchAsync(string keyword)
        {
            var items = await _knowledgeItemRepository.SearchAsync(keyword);

            return items.Select(k => new KnowledgeItemDto
            {
                ItemId = k.ItemId,
                Title = k.Title,
                Description = !string.IsNullOrEmpty(k.Description) && k.Description.Length > 200
                    ? k.Description.Substring(0, 200) + "..."
                    : k.Description,
                DomainId = k.DomainId,
                DomainName = k.Domain?.DomainName ?? string.Empty,
                CategoryId = k.CategoryId,
                CategoryName = k.Category?.CategoryName ?? string.Empty,
                OwnerId = k.OwnerId,
                OwnerName = k.Owner?.Name ?? "Unknown",
                CreatedOn = k.CreatedOn ?? DateTime.MinValue,
                Framework = k.Framework,
                Language = k.Language,
                Tags = k.KnowledgeTags.Select(t => t.TagName).ToList(),
                EngagementScore = k.Engagements?.Count ?? 0,
                Status = k.Status ?? "Draft",
                IsEventItem = k.IsEventItem ?? false
            }).ToList();
        }
    }
}

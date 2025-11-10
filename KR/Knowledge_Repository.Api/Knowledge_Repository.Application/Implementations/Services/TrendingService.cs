using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    /// <summary>
    /// Service for retrieving trending knowledge items based on engagement.
    /// </summary>
    public class TrendingService : ITrendingService
    {
        private readonly IKnowledgeItemRepository _knowledgeItemRepository;
        private readonly ILogger<TrendingService> _logger;

        public TrendingService(
            IKnowledgeItemRepository knowledgeItemRepository,
            ILogger<TrendingService> logger)
        {
            _knowledgeItemRepository = knowledgeItemRepository ?? throw new ArgumentNullException(nameof(knowledgeItemRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets top trending knowledge items, ranked by engagement score and creation date.
        /// </summary>
        public async Task<List<KnowledgeItemDto>> GetTrendingAsync(int top = 5)
        {
            try
            {
                var items = await _knowledgeItemRepository.GetAllWithDomainAndEngagementsAsync();

                var trendingItems = items
                    .OrderByDescending(k => k.Engagements?.Count ?? 0)
                    .ThenByDescending(k => k.CreatedOn)
                    .Take(top)
                    .Select(k => new KnowledgeItemDto
                    {
                        ItemId = k.ItemId,
                        Title = k.Title,
                        Description = k.Description,
                        DomainId = k.DomainId,
                        DomainName = k.Domain?.DomainName ?? string.Empty,
                        CategoryId = k.CategoryId,
                        CategoryName = k.Category?.CategoryName ?? string.Empty,
                        Language = k.Language,
                        Framework = k.Framework,
                        OwnerId = k.OwnerId,
                        OwnerName = k.Owner?.Name ?? "Unknown",
                        CreatedOn = k.CreatedOn ?? DateTime.MinValue,
                        Tags = k.KnowledgeTags?.Select(t => t.TagName).ToList() ?? new List<string>(),
                        EngagementScore = k.Engagements?.Count ?? 0,
                        Status = k.Status ?? "Draft"
                    })
                    .ToList();

                return trendingItems;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trending knowledge items");
                throw;
            }
        }
    }
}

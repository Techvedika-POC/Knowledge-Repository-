using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class TopicHighlightService : ITopicHighlightService
    {
        private readonly IKnowledgeItemRepository _knowledgeItemRepository;
        private readonly IDomainRepository _domainRepository;
        private readonly ILogger<TopicHighlightService> _logger;

        public TopicHighlightService(
            IKnowledgeItemRepository knowledgeItemRepository,
            IDomainRepository domainRepository,
            ILogger<TopicHighlightService> logger)
        {
            _knowledgeItemRepository = knowledgeItemRepository ?? throw new ArgumentNullException(nameof(knowledgeItemRepository));
            _domainRepository = domainRepository ?? throw new ArgumentNullException(nameof(domainRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ----------------------------------------------------------------------
        // 🔹 Get top topic highlights based on engagement and recency
        // ----------------------------------------------------------------------
        public async Task<List<TopicHighlightDto>> GetTopicHighlightsAsync(int top = 10)
        {
            try
            {
                var items = await _knowledgeItemRepository.GetAllWithDomainAndEngagementsAsync();

                var topics = items
                    .Where(k => k.Domain != null)
                    .GroupBy(k => k.Domain!.DomainName)
                    .Select(g => new TopicHighlightDto
                    {
                        TopicName = g.Key,
                        RecentItemCount = g.Count(),
                        ExampleContributors = g
                            .Select(k => k.Owner?.Name ?? "Unknown")
                            .Distinct()
                            .Take(3)
                            .ToArray(),
                        EngagementScore = g.Sum(k => k.Engagements?.Count ?? 0)
                    })
                    .OrderByDescending(t => t.EngagementScore)
                    .ThenByDescending(t => t.RecentItemCount)
                    .Take(top)
                    .ToList();

                return topics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching topic highlights");
                throw;
            }
        }

        // ----------------------------------------------------------------------
        // 🔹 Get Knowledge Items by Domain Name
        // ----------------------------------------------------------------------
        public async Task<List<KnowledgeItemDto>> GetKnowledgeItemsByDomainAsync(string domainName, int top = 10)
        {
            if (string.IsNullOrWhiteSpace(domainName))
                throw new ArgumentException("Domain name must be provided.", nameof(domainName));

            try
            {
                var items = await _knowledgeItemRepository.GetByDomainNameAsync(domainName);

                return items
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
                        EngagementScore = k.Engagements?.Count ?? 0
                    })
                    .OrderByDescending(k => k.EngagementScore)
                    .ThenByDescending(k => k.CreatedOn)
                    .Take(top)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching knowledge items by domain {DomainName}", domainName);
                throw;
            }
        }
    }
}

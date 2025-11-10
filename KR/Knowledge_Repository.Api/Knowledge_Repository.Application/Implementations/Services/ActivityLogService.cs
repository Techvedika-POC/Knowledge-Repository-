using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IKnowledgeItemRepository _knowledgeItemRepository;
        private readonly IEngagementRepository _engagementRepository;

        public ActivityLogService(
            IKnowledgeItemRepository knowledgeItemRepository,
            IEngagementRepository engagementRepository)
        {
            _knowledgeItemRepository = knowledgeItemRepository;
            _engagementRepository = engagementRepository;
        }

        public async Task<IEnumerable<ActivityLogDto>> GetUserContributionsAsync(Guid userId)
        {
            // Get all knowledge items owned by the user
            var items = await _knowledgeItemRepository.GetByOwnerAsync(userId);

            return items
                .OrderByDescending(k => k.CreatedOn)
                .Select(MapToActivityLogDto);
        }

        public async Task<ActivityLogDto> GetContributionDetailsAsync(Guid itemId)
        {
            var item = await _knowledgeItemRepository.GetByIdWithDetailsAsync(itemId);
            if (item == null) return null;

            return MapToActivityLogDto(item);
        }

        public async Task<IEnumerable<ActivityLogDto>> GetUserContributionsFilteredAsync(
            Guid userId, string domain = null, string category = null, string title = null,
            string status = null, DateTime? date = null)
        {
            var items = await _knowledgeItemRepository.GetByOwnerAsync(userId);
            var filtered = items.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(domain))
                filtered = filtered.Where(k => k.Domain != null && k.Domain.DomainName.Contains(domain, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(category))
                filtered = filtered.Where(k => k.Category != null && k.Category.CategoryName.Contains(category, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(title))
                filtered = filtered.Where(k => !string.IsNullOrEmpty(k.Title) && k.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(status))
                filtered = filtered.Where(k => !string.IsNullOrEmpty(k.Status) && k.Status.Contains(status, StringComparison.OrdinalIgnoreCase));

            if (date.HasValue)
            {
                var start = date.Value.Date;
                var end = start.AddDays(1);
                filtered = filtered.Where(k => k.CreatedOn >= start && k.CreatedOn < end);
            }

            return filtered
                .OrderByDescending(k => k.CreatedOn)
                .Select(MapToActivityLogDto)
                .ToList();
        }

        public async Task<IEnumerable<string>> GetUserDomainsAsync(Guid userId)
        {
            var items = await _knowledgeItemRepository.GetByOwnerAsync(userId);
            return items
                .Where(k => k.Domain != null)
                .Select(k => k.Domain.DomainName)
                .Distinct();
        }

        public async Task<IEnumerable<string>> GetUserCategoriesAsync(Guid userId)
        {
            var items = await _knowledgeItemRepository.GetByOwnerAsync(userId);
            return items
                .Where(k => k.Category != null)
                .Select(k => k.Category.CategoryName)
                .Distinct();
        }

        public async Task<IEnumerable<string>> GetUserTitlesAsync(Guid userId)
        {
            var items = await _knowledgeItemRepository.GetByOwnerAsync(userId);
            return items
                .Select(k => k.Title)
                .Distinct();
        }

        public async Task<PagedResult<ActivityLogDto>> GetUserContributionsPagedAsync(
            Guid userId, int pageNumber = 1, int pageSize = 10,
            string domain = null, string category = null, string title = null,
            string status = null, DateTime? date = null)
        {
            var filtered = await GetUserContributionsFilteredAsync(userId, domain, category, title, status, date);
            var totalCount = filtered.Count();

            var items = filtered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<ActivityLogDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<IEnumerable<ActivityLogDto>> GetUserContributionsThisMonthAsync(Guid userId)
        {
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var items = await _knowledgeItemRepository.GetByOwnerAsync(userId);

            return items
                .Where(k => k.CreatedOn >= startOfMonth)
                .OrderByDescending(k => k.CreatedOn)
                .Select(MapToActivityLogDto);
        }

        public async Task<IEnumerable<KnowledgeItemDto>> GetUserFavouritesAsync(Guid userId)
        {
            var favourites = await _engagementRepository.GetFavouritesByUserAsync(userId);

            return favourites.Select(e => MapToKnowledgeItemDto(e.Item));
        }

        // ------------------ Helpers ------------------
        private static ActivityLogDto MapToActivityLogDto(KnowledgeItem k)
        {
            return new ActivityLogDto
            {
                UserId = k.OwnerId,
                ItemId = k.ItemId,
                Title = k.Title,
                Category = k.Category?.CategoryName,
                Domain = k.Domain?.DomainName,
                Description = k.Description != null && k.Description.Length > 100 ? k.Description.Substring(0, 100) + "..." : k.Description,
                Status = k.Status,
                Date = k.CreatedOn
            };
        }

        private static KnowledgeItemDto MapToKnowledgeItemDto(KnowledgeItem k)
        {
            return new KnowledgeItemDto
            {
                ItemId = k.ItemId,
                Title = k.Title,
                Description = k.Description,
                DomainId = k.DomainId,
                DomainName = k.Domain?.DomainName,
                CategoryId = k.CategoryId,
                CategoryName = k.Category?.CategoryName,
                OwnerId = k.OwnerId,
                OwnerName = k.Owner?.Name,
                Status = k.Status,
                CreatedOn = k.CreatedOn ?? DateTime.UtcNow,
                Tags = k.KnowledgeTags?.Select(t => t.TagName).ToList() ?? new List<string>(),
                Views = k.Engagements.Count(e => e.EngagementType == "View"),
                Likes = k.Engagements.Count(e => e.EngagementType == "Like"),
                Comments = k.Engagements.Count(e => e.EngagementType == "Comment")
            };
        }
    }
}

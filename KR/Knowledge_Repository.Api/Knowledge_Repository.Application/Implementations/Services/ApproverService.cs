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
    /// <summary>
    /// Provides functionality for approvers to manage knowledge item approvals and rejections.
    /// </summary>
    public class ApproverService : IApproverService
    {
        private readonly IKnowledgeItemRepository _knowledgeItemRepository;
        private readonly IUserRepository _userRepository;

        public ApproverService(
            IKnowledgeItemRepository knowledgeItemRepository,
            IUserRepository userRepository)
        {
            _knowledgeItemRepository = knowledgeItemRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Get all pending knowledge items.
        /// </summary>
        public async Task<List<KnowledgeItemDto>> GetPendingKnowledgeItemsAsync()
        {
            var pendingItems = await _knowledgeItemRepository.GetPendingItemsAsync(1, int.MaxValue);

            return pendingItems.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Approve a specific knowledge item.
        /// </summary>
        public async Task<bool> ApproveKnowledgeItemAsync(Guid itemId, Guid approverId)
        {
            return await _knowledgeItemRepository.ApproveItemAsync(itemId, approverId);
        }

        /// <summary>
        /// Reject a specific knowledge item.
        /// </summary>
        public async Task<bool> RejectKnowledgeItemAsync(Guid itemId, Guid approverId)
        {
            return await _knowledgeItemRepository.RejectItemAsync(itemId, approverId);
        }

        /// <summary>
        /// Get a paginated list of pending knowledge items.
        /// </summary>
        public async Task<(List<KnowledgeItemDto> Items, int TotalCount)> GetPendingKnowledgeItemsAsync(int pageNumber, int pageSize)
        {
            var pendingItems = await _knowledgeItemRepository.GetPendingItemsAsync(pageNumber, pageSize);
            var totalCount = await _knowledgeItemRepository.GetPendingItemsCountAsync();

            var dtos = pendingItems.Select(MapToDto).ToList();
            return (dtos, totalCount);
        }
        private KnowledgeItemDto MapToDto(KnowledgeItem k)
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
                Framework = k.Framework,
                Language = k.Language,
                Metadata = k.Metadata,
                CreatedBy = k.CreatedBy,
                UpdatedBy = k.UpdatedBy,
                CreatedOn = k.CreatedOn ?? DateTime.UtcNow,
                UpdatedOn = k.UpdatedOn
            };
        }
    }
}

using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    /// <summary>
    /// Infrastructure-backed implementation of IKnowledgeItemService.
    /// Orchestrates repositories and file storage without direct environment or DbContext dependencies.
    /// </summary>
    public class KnowledgeItemService : IKnowledgeItemService
    {
        private readonly IKnowledgeItemRepository _knowledgeItemRepository;
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<KnowledgeItemService> _logger;

        public KnowledgeItemService(
            IKnowledgeItemRepository knowledgeItemRepository,
            IAttachmentRepository attachmentRepository,
            IFileStorageService fileStorageService,
            ILogger<KnowledgeItemService> logger)
        {
            _knowledgeItemRepository = knowledgeItemRepository;
            _attachmentRepository = attachmentRepository;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        // -------------------------------------------------------------------
        // GET DETAILS
        // -------------------------------------------------------------------
        public async Task<KnowledgeItemDetailsDto?> GetKnowledgeItemDetailsAsync(Guid itemId)
        {
            var item = await _knowledgeItemRepository.GetByIdWithDetailsAsync(itemId);
            if (item == null) return null;

            return new KnowledgeItemDetailsDto
            {
                ItemId = item.ItemId,
                Title = item.Title ?? "",
                Description = item.Description ?? "",
                ContributorName = item.Owner?.Name ?? "",
                EngagementScore = item.Engagements?.Count ?? 0,
                CreatedOn = item.CreatedOn ?? DateTime.MinValue,
                Tags = item.KnowledgeTags?.Select(t => t.TagName).ToList() ?? new List<string>(),
                Attachments = item.Attachments?.Select(a => new AttachmentDto
                {
                    FileName = a.FileName ?? "",
                    MimeType = a.MimeType ?? "",
                    FileUrl = a.FilePath ?? "",
                    FileSize = a.FileSize ?? 0
                }).ToList() ?? new List<AttachmentDto>(),
                Language = item.Language ?? "",
                Framework = item.Framework ?? "",
                Metadata = item.Metadata ?? "",
                Visibility = ExtractVisibility(item.Metadata),
                CategoryName = item.Category?.CategoryName ?? "",
                DomainName = item.Domain?.DomainName ?? "",
                OwnerName = item.Owner?.Name ?? ""
            };
        }

        private static string ExtractVisibility(string? metadata)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(metadata))
                    return "Private";

                var json = JsonSerializer.Deserialize<JsonElement>(metadata);
                return json.TryGetProperty("Visibility", out var v)
                    ? v.GetString() ?? "Private"
                    : "Private";
            }
            catch
            {
                return "Private";
            }
        }

        // -------------------------------------------------------------------
        // UPLOAD
        // -------------------------------------------------------------------
        public async Task<KnowledgeItem> UploadKnowledgeItemAsync(KnowledgeItemUploadDto dto, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || dto.DomainId == Guid.Empty || dto.CategoryId == Guid.Empty)
                throw new ArgumentException("Title, Domain, and Category are required.");

            var knowledgeItem = new KnowledgeItem
            {
                ItemId = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                DomainId = dto.DomainId,
                CategoryId = dto.CategoryId,
                OwnerId = userId,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedOn = DateTime.UtcNow,
                UpdatedBy = userId,
                Status = "Pending",
                Language = JsonSerializer.Serialize(dto.Language ?? new List<string>()),
                Framework = JsonSerializer.Serialize(dto.Framework ?? new List<string>()),
                Metadata = JsonSerializer.Serialize(new { Visibility = dto.Visibility ?? "Private" }),
                IsEventItem = dto.IsEventItem
            };

            await _knowledgeItemRepository.AddAsync(knowledgeItem);

            if (dto.Attachments?.Count > 0)
            {
                foreach (var file in dto.Attachments)
                {
                    var fileUrl = await _fileStorageService.SaveFileAsync(file.FileData, file.FileName);

                    var attachment = new Attachment
                    {
                        AttachmentId = Guid.NewGuid(),
                        ItemId = knowledgeItem.ItemId,
                        FileName = file.FileName,
                        FilePath = fileUrl,
                        MimeType = file.MimeType,
                        FileSize = file.FileSize,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = userId
                    };

                    await _attachmentRepository.AddAsync(attachment);
                }
            }

            return knowledgeItem;
        }

        // -------------------------------------------------------------------
        // FILTERED FETCH
        // -------------------------------------------------------------------
        public async Task<IEnumerable<KnowledgeItemDto>> GetKnowledgeItemsAsync(Guid? domainId = null, Guid? categoryId = null)
        {
            var items = await _knowledgeItemRepository.GetByDomainOrCategoryAsync(domainId, categoryId);

            return items.Select(k => new KnowledgeItemDto
            {
                ItemId = k.ItemId,
                Title = k.Title,
                Description = k.Description,
                DomainName = k.Domain?.DomainName ?? "",
                CategoryName = k.Category?.CategoryName ?? "",
                SubmittedBy = k.Owner?.Name ?? "Unknown",
                CreatedOn = k.CreatedOn ?? DateTime.MinValue,
                Framework = k.Framework,
                Language = k.Language
            }).ToList();
        }

        // -------------------------------------------------------------------
        // OWNER FETCH
        // -------------------------------------------------------------------
        public async Task<IEnumerable<KnowledgeItemDto>> GetKnowledgeItemsByOwnerAsync(Guid ownerId)
        {
            var items = await _knowledgeItemRepository.GetByOwnerAsync(ownerId);

            return items.Select(k => new KnowledgeItemDto
            {
                ItemId = k.ItemId,
                Title = k.Title,
                Description = k.Description,
                DomainName = k.Domain?.DomainName ?? "",
                CategoryName = k.Category?.CategoryName ?? "",
                SubmittedBy = k.Owner?.Name ?? "Unknown",
                CreatedOn = k.CreatedOn ?? DateTime.MinValue,
                Framework = k.Framework,
                Language = k.Language
            }).ToList();
        }

        // -------------------------------------------------------------------
        // FRESH PICKS
        // -------------------------------------------------------------------
        public async Task<IEnumerable<KnowledgeItemDto>> GetFreshPicksAsync(int count = 10)
        {
            var items = await _knowledgeItemRepository.GetFreshPicksAsync(count);

            return items.Select(k => new KnowledgeItemDto
            {
                ItemId = k.ItemId,
                Title = k.Title,
                Description = k.Description,
                DomainName = k.Domain?.DomainName ?? "",
                CategoryName = k.Category?.CategoryName ?? "",
                SubmittedBy = k.Owner?.Name ?? "Unknown",
                CreatedOn = k.CreatedOn ?? DateTime.MinValue,
                Framework = k.Framework,
                Language = k.Language
            }).ToList();
        }
    }
}

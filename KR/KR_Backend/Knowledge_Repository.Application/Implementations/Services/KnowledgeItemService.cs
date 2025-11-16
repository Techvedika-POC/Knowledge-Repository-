using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{

    public class KnowledgeItemService : IKnowledgeItemService
    {
        private readonly IKnowledgeItemRepository _knowledgeItemRepository;
        private readonly IKnowledgeVersionRepository _knowledgeVersionRepository;
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly IKnowledgeTagRepository _knowledgeTagRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IEventKnowledgeItemRepository _eventKnowledgeItemRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<KnowledgeItemService> _logger;


        public KnowledgeItemService(
           IKnowledgeItemRepository knowledgeItemRepository,
            IKnowledgeVersionRepository knowledgeVersionRepository,
            IAttachmentRepository attachmentRepository,
            IKnowledgeTagRepository knowledgeTagRepository,
            ITeamRepository teamRepository,
            IEventKnowledgeItemRepository eventKnowledgeItemRepository,
            IUserRepository userRepository,
            IFileStorageService fileStorageService,
            ILogger<KnowledgeItemService> logger
            )
        {
            _knowledgeItemRepository = knowledgeItemRepository;
            _knowledgeVersionRepository = knowledgeVersionRepository;
            _attachmentRepository = attachmentRepository;
            _knowledgeTagRepository = knowledgeTagRepository;
            _teamRepository = teamRepository;
            _eventKnowledgeItemRepository = eventKnowledgeItemRepository;
            _userRepository = userRepository;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

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


        public async Task<KnowledgeItemDto> UploadKnowledgeItemAsync(KnowledgeItemUploadDto dto, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title is required.");

            var knowledgeItem = await _knowledgeItemRepository.GetByTitleAndUserAsync(dto.Title, userId);

            int newVersionNumber = 1;

            if (knowledgeItem != null)
            {
                knowledgeItem.Description = dto.Description;
                knowledgeItem.UpdatedOn = DateTime.UtcNow;
                knowledgeItem.UpdatedBy = userId;
                await _knowledgeItemRepository.UpdateAsync(knowledgeItem);

                var lastVersion = await _knowledgeVersionRepository.GetLastVersionByItemIdAsync(knowledgeItem.ItemId);
                newVersionNumber = (lastVersion?.VersionNumber ?? 0) + 1;
            }
            else
            {
                knowledgeItem = new KnowledgeItem
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
                    IsEventItem = dto.IsEventItem,
                    Language = dto.Language != null ? System.Text.Json.JsonSerializer.Serialize(dto.Language) : null,
                    Framework = dto.Framework != null ? System.Text.Json.JsonSerializer.Serialize(dto.Framework) : null,
                    Metadata = System.Text.Json.JsonSerializer.Serialize(new { Visibility = dto.Visibility ?? "Private" })
                };
                await _knowledgeItemRepository.AddAsync(knowledgeItem);
            }

            var version = new KnowledgeVersion
            {
                VersionId = Guid.NewGuid(),
                ItemId = knowledgeItem.ItemId,
                VersionNumber = newVersionNumber,
                ChangesSummary = newVersionNumber == 1 ? "Initial version" : "Updated version",
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow
            };
            await _knowledgeVersionRepository.AddAsync(version);

            if (dto.Attachments?.Count > 0)
            {
                foreach (var file in dto.Attachments)
                {
                    string publicPath = await _fileStorageService.SaveFileAsync(file.FileData, file.FileName);

                    var attachment = new Attachment
                    {
                        AttachmentId = Guid.NewGuid(),
                        ItemId = knowledgeItem.ItemId,
                        VersionId = version.VersionId,
                        FileName = file.FileName,
                        FilePath = publicPath, 
                        MimeType = file.MimeType,
                        FileSize = file.FileSize,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = userId,
                        UpdatedOn = DateTime.UtcNow,
                        UpdatedBy = userId
                    };

                    await _attachmentRepository.AddAsync(attachment);
                }
            }

            if (dto.Tags?.Count > 0)
            {
                var tags = dto.Tags.Select(tag => new KnowledgeTag
                {
                    TagId = Guid.NewGuid(),
                    ItemId = knowledgeItem.ItemId,
                    VersionId = version.VersionId,
                    TagName = tag.Trim(),
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = userId,
                    UpdatedOn = DateTime.UtcNow,
                    UpdatedBy = userId
                }).ToList();

                await _knowledgeTagRepository.AddRangeAsync(tags);
            }

            if (dto.IsEventItem && dto.EventId.HasValue)
            {
                var team = await _teamRepository.GetTeamByEventAndUserAsync(dto.EventId.Value, userId);
                if (team == null)
                    throw new Exception("You are not part of any team for this event.");

                var eventKnowledgeItem = new EventKnowledgeItem
                {
                    EventItemId = Guid.NewGuid(),
                    EventId = dto.EventId.Value,
                    ItemId = knowledgeItem.ItemId,
                    TeamId = team.TeamId,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = userId,
                    UpdatedOn = DateTime.UtcNow
                };
                await _eventKnowledgeItemRepository.AddAsync(eventKnowledgeItem);
            }

            return new KnowledgeItemDto
            {
                ItemId = knowledgeItem.ItemId,
                Title = knowledgeItem.Title,
                Description = knowledgeItem.Description,
                Version = newVersionNumber
            };
        }


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
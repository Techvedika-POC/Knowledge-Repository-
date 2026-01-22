using Knowledge_Repository.Application.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
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
  
        private readonly IHttpContextAccessor _httpContextAccessor;

        public KnowledgeItemService(
            IKnowledgeItemRepository knowledgeItemRepository,
            IKnowledgeVersionRepository knowledgeVersionRepository,
            IAttachmentRepository attachmentRepository,
            IKnowledgeTagRepository knowledgeTagRepository,
            ITeamRepository teamRepository,
            IEventKnowledgeItemRepository eventKnowledgeItemRepository,
            IUserRepository userRepository,
            IFileStorageService fileStorageService,
            ILogger<KnowledgeItemService> logger,
            IHttpContextAccessor httpContextAccessor)
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
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
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
                DomainId = item.DomainId,
                CategoryId = item.CategoryId,
                IsEventItem = item.IsEventItem ?? false,
                EventId = item.EventKnowledgeItems?.FirstOrDefault()?.EventId, // optional
                ContributorName = item.Owner?.Name ?? "",
                EngagementScore = item.Engagements?.Count ?? 0,
                CreatedOn = item.CreatedOn ?? DateTime.MinValue,
                Tags = item.KnowledgeTags?.Select(t => t.TagName).ToList() ?? new List<string>(),
                Attachments = item.Attachments?.Select(a => new AttachmentDto
                {
                    AttachmentId = a.AttachmentId,
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
        public async Task<KnowledgeItemDto> UpdateKnowledgeItemAsync(
     Guid itemId,
     KnowledgeItemUpdateDto dto,
     Guid userId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

         
            var knowledgeItem = await _knowledgeItemRepository.GetByIdWithDetailsAsync(itemId);
            if (knowledgeItem == null)
                throw new KeyNotFoundException("Knowledge item not found.");


            if (knowledgeItem.OwnerId != userId)
                throw new UnauthorizedAccessException("You are not authorized to update this item.");

          
            try
            {
              
                var changed = false;

                if (!string.IsNullOrWhiteSpace(dto.Title) && dto.Title != knowledgeItem.Title)
                {
                    knowledgeItem.Title = dto.Title.Trim();
                    changed = true;
                }

                if (dto.Description != null && dto.Description != knowledgeItem.Description)
                {
                    knowledgeItem.Description = dto.Description;
                    changed = true;
                }

                if (dto.DomainId.HasValue && dto.DomainId.Value != knowledgeItem.DomainId)
                {
                    knowledgeItem.DomainId = dto.DomainId.Value;
                    changed = true;
                }

                if (dto.CategoryId.HasValue && dto.CategoryId.Value != knowledgeItem.CategoryId)
                {
                    knowledgeItem.CategoryId = dto.CategoryId.Value;
                    changed = true;
                }

                if (!string.IsNullOrEmpty(dto.Status) && dto.Status != knowledgeItem.Status)
                {
                    knowledgeItem.Status = dto.Status;
                    changed = true;
                }


                if (dto.Framework != null && dto.Framework.Any())
                {
                    knowledgeItem.Framework = JsonSerializer.Serialize(dto.Framework);
                    changed = true;
                }

                if (dto.Language != null && dto.Language.Any())
                {
                    knowledgeItem.Language = JsonSerializer.Serialize(dto.Language);
                    changed = true;
                }

          
                if (!string.IsNullOrWhiteSpace(dto.Visibility))
                {
                    try
                    {
                        var metadataDict = string.IsNullOrWhiteSpace(knowledgeItem.Metadata)
                            ? new Dictionary<string, string>()
                            : JsonSerializer.Deserialize<Dictionary<string, string>>(knowledgeItem.Metadata) ?? new Dictionary<string, string>();

                        metadataDict["Visibility"] = dto.Visibility;
                        knowledgeItem.Metadata = JsonSerializer.Serialize(metadataDict);
                        changed = true;
                    }
                    catch
                    {
                        knowledgeItem.Metadata = JsonSerializer.Serialize(new { Visibility = dto.Visibility });
                        changed = true;
                    }
                }

                
                if (!string.IsNullOrWhiteSpace(dto.KnowledgeText))
                {
                    knowledgeItem.KnowledgeText = dto.KnowledgeText;
                    changed = true;
                }

                if (dto.Embedding != null && dto.Embedding.Any())
                {
                    knowledgeItem.Embedding = dto.Embedding;
                    changed = true;
                }

              
                knowledgeItem.UpdatedOn = DateTime.UtcNow;
                knowledgeItem.UpdatedBy = userId;

                if (changed)
                    await _knowledgeItemRepository.UpdateAsync(knowledgeItem);
                
                var lastVersion = await _knowledgeVersionRepository.GetLastVersionByItemIdAsync(knowledgeItem.ItemId);
                var newVersionNumber = (lastVersion?.VersionNumber ?? 0) + 1;

                var changesSummary = string.IsNullOrWhiteSpace(dto.ChangesSummary)
                    ? $"Updated by {userId} at {DateTime.UtcNow:O}"
                    : dto.ChangesSummary.Trim();

                var newVersion = new KnowledgeVersion
                {
                    VersionId = Guid.NewGuid(),
                    ItemId = knowledgeItem.ItemId,
                    VersionNumber = newVersionNumber,
                    ChangesSummary = changesSummary,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow
                };

                await _knowledgeVersionRepository.AddAsync(newVersion);


                if (dto.Tags != null && dto.Tags.Any())
                {
                    var tagEntities = dto.Tags
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .Select(t => new KnowledgeTag
                        {
                            TagId = Guid.NewGuid(),
                            ItemId = knowledgeItem.ItemId,
                            VersionId = newVersion.VersionId,
                            TagName = t.Trim(),
                            CreatedOn = DateTime.UtcNow,
                            CreatedBy = userId,
                            UpdatedOn = DateTime.UtcNow,
                            UpdatedBy = userId
                        })
                        .ToList();

                    if (tagEntities.Count > 0)
                        await _knowledgeTagRepository.AddRangeAsync(tagEntities);
                }
                var replace = dto.ReplaceAttachments; 

                if (replace)
                {
                    var existingAttachments = await _attachmentRepository.GetByItemIdAsync(knowledgeItem.ItemId);
                    if (existingAttachments != null && existingAttachments.Any())
                    {
                        foreach (var att in existingAttachments)
                        {
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(att.FilePath))
                                {
                                    await _fileStorageService.DeleteFileAsync(att.FilePath);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogWarning(ex, "Failed to delete storage file {Path}", att.FilePath);
                            }

                            await _attachmentRepository.DeleteAsync(att);

                        }
                    }
                }
                if (dto.Attachments != null && dto.Attachments.Count > 0)
                {
                    foreach (var file in dto.Attachments)
                    {
                        var publicPath = await _fileStorageService.SaveFileAsync(file.FileData, file.FileName);

                        var attachment = new Attachment
                        {
                            AttachmentId = Guid.NewGuid(),
                            ItemId = knowledgeItem.ItemId,
                            VersionId = newVersion.VersionId,
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

                
                return new KnowledgeItemDto
                {
                    ItemId = knowledgeItem.ItemId,
                    Title = knowledgeItem.Title,
                    Description = knowledgeItem.Description,
                    Version = newVersionNumber
                };
            }
            catch
            {
                
                throw;
            }
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
                Language = k.Language,
                Tags = k.KnowledgeTags?.Select(t => t.TagName).ToList() ?? new List<string>(),
                EngagementScore = k.Engagements?.Count ?? 0
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
                Language = k.Language,
                Tags = k.KnowledgeTags?.Select(t => t.TagName).ToList() ?? new List<string>(),
                EngagementScore = k.Engagements?.Count ?? 0
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
                Language = k.Language,
                Tags = k.KnowledgeTags?.Select(t => t.TagName).ToList() ?? new List<string>(),
                EngagementScore = k.Engagements?.Count ?? 0
            }).ToList();
        }
        public async Task<List<VersionWithAttachmentsDto>> GetVersionsWithFilesAsync(Guid itemId)
        {
            var versions = await _knowledgeItemRepository.GetVersionsWithAttachmentsAsync(itemId);

            if (versions == null || !versions.Any())
                return new List<VersionWithAttachmentsDto>();

            return versions
                .OrderByDescending(v => v.VersionNumber)
                .Select(v => new VersionWithAttachmentsDto
                {
                    VersionId = v.VersionId,
                    VersionNumber = v.VersionNumber,
                    CreatedOn = v.CreatedOn,
                    ChangesSummary = v.ChangesSummary,
                    Attachments = v.Attachments
                        .Select(a => new AttachmentDto
                        {
                            AttachmentId = a.AttachmentId,
                            FileName = a.FileName,
                            MimeType = a.MimeType,
                            FileSize = a.FileSize,
                            FileUrl = a.FilePath,
                            FilePath = a.FilePath
                        })
                        .ToList()
                })
                .ToList();
        }
        public async Task<List<KnowledgeItemDto>> GetApprovedEventItemsAsync()
        {
            var items = await _knowledgeItemRepository.GetApprovedEventItemsAsync();

            return items.Select(k => new KnowledgeItemDto
            {
                ItemId = k.ItemId,
                Title = k.Title,
                Description = k.Description,
                CreatedOn = k.CreatedOn ?? DateTime.MinValue,

                DomainId = k.DomainId,
                DomainName = k.Domain?.DomainName,
                CategoryId = k.CategoryId,
                CategoryName = k.Category?.CategoryName,

                OwnerId = k.OwnerId,
                OwnerName = k.Owner?.Name,
                SubmittedBy = k.Owner?.Name,

                Status = k.Status,
                Visibility = ExtractVisibility(k.Metadata),

                IsEventItem = true, 

                Tags = k.KnowledgeTags?.Select(t => t.TagName).ToList() ?? new List<string>(),
                Framework = k.Framework,
                Language = k.Language,
                EngagementScore = k.Engagements?.Count ?? 0

            }).ToList();
        }
        public async Task<AttachmentInfoDto?> GetAttachmentByIdAsync(Guid attachmentId)
        {
            var att = await _attachmentRepository.GetAttachmentByIdAsync(attachmentId);
            if (att == null) return null;

            return new AttachmentInfoDto
            {
                AttachmentId = att.AttachmentId,
                FileName = att.FileName,
                MimeType = att.MimeType,
                FileSize = att.FileSize,
                FilePath = att.FilePath, 
                FileUrl = BuildPublicFileUrl(att.FilePath),
                FileData = att.FileData 
            };
        }
        private string? BuildPublicFileUrl(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return null;
          
            var trimmed = filePath.TrimStart('/', '\\');
            return "/" + trimmed; 
        }

    }
}
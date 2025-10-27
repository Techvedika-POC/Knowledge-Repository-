using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Implementations
{
    /// <summary>
    /// Service responsible for all Knowledge Item operations:
    /// creation, retrieval, filtering, attachments, and events.
    /// </summary>
    public class KnowledgeItemService : IKnowledgeItemService
    {
        private readonly Knowledge_Repository_dbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<KnowledgeItemService> _logger;

        public KnowledgeItemService(
            Knowledge_Repository_dbContext context,
            IWebHostEnvironment env,
            ILogger<KnowledgeItemService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Knowledge Item Retrieval

        public async Task<KnowledgeItemDetailsDto?> GetKnowledgeItemDetailsAsync(Guid itemId)
        {
            var item = await _context.KnowledgeItems
                .Include(k => k.Category)
                .Include(k => k.Domain)
                .Include(k => k.Owner)
                .Include(k => k.KnowledgeTags)
                .Include(k => k.Attachments)
                .FirstOrDefaultAsync(k => k.ItemId == itemId);

            if (item == null) return null;

            return new KnowledgeItemDetailsDto
            {
                ItemId = item.ItemId,
                Title = item.Title ?? "",
                Description = item.Description ?? "",
                ContributorName = item.Owner?.Name ?? "",
                EngagementScore = 0,
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
                Visibility = string.Empty,
                CategoryName = item.Category?.CategoryName ?? "",
                DomainName = item.Domain?.DomainName ?? "",
                OwnerName = item.Owner?.Name ?? ""
            };
        }

        #endregion

        #region Knowledge Item Upload

        public async Task<KnowledgeItem> UploadKnowledgeItemAsync(KnowledgeItemUploadDto dto, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || dto.DomainId == Guid.Empty || dto.CategoryId == Guid.Empty)
                throw new ArgumentException("Title, Domain, and Category are required.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create Knowledge Item
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
                    IsEventItem = dto.IsEventItem,
                    Language = JsonSerializer.Serialize(dto.Language ?? new List<string>()),
                    Framework = JsonSerializer.Serialize(dto.Framework ?? new List<string>()),
                    Metadata = JsonSerializer.Serialize(new { Visibility = dto.Visibility ?? "Private" })
                };
                _context.KnowledgeItems.Add(knowledgeItem);

                // ----------------- 2️ Create Initial Version -----------------
                var version = new KnowledgeVersion
                {
                    VersionId = Guid.NewGuid(),
                    ItemId = knowledgeItem.ItemId,
                    VersionNumber = 1,
                    ChangesSummary = "Initial version",
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow
                };
                _context.KnowledgeVersions.Add(version);
                await _context.SaveChangesAsync();

                // ----------------- 3️Handle Attachments -----------------
                var uploadsRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot"), "uploads");
                Directory.CreateDirectory(uploadsRoot);

                if (dto.Attachments?.Count > 0)
                {
                    foreach (var file in dto.Attachments)
                    {
                        if (file.FileData?.Length > 0)
                        {
                            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                            var diskPath = Path.Combine(uploadsRoot, uniqueFileName);
                            await File.WriteAllBytesAsync(diskPath, file.FileData);

                            var attachment = new Attachment
                            {
                                AttachmentId = Guid.NewGuid(),
                                ItemId = knowledgeItem.ItemId,
                                VersionId = version.VersionId,
                                FileName = file.FileName,
                                FilePath = $"/uploads/{uniqueFileName}",
                                MimeType = file.MimeType,
                                FileSize = file.FileSize,
                                FileType = Path.GetExtension(file.FileName)?.ToLower() ?? "unknown",
                                CreatedOn = DateTime.UtcNow,
                                CreatedBy = userId,
                                UpdatedOn = DateTime.UtcNow,
                                UpdatedBy = userId
                            };
                            _context.Attachments.Add(attachment);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // ----------------- 4️ Add Tags -----------------
                foreach (var tag in dto.Tags ?? new List<string>())
                {
                    _context.KnowledgeTags.Add(new KnowledgeTag
                    {
                        TagId = Guid.NewGuid(),
                        ItemId = knowledgeItem.ItemId,
                        VersionId = version.VersionId,
                        TagName = tag.Trim(),
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = userId,
                        UpdatedOn = DateTime.UtcNow,
                        UpdatedBy = userId
                    });
                }
                await _context.SaveChangesAsync();

                // ----------------- 5️ Generate Knowledge Item Embedding -----------------
        
                // ----------------- 6️ Create Activity Log -----------------
                _context.ActivityLogs.Add(new ActivityLog
                {
                    ActivityId = Guid.NewGuid(),
                    UserId = userId,
                    ItemId = knowledgeItem.ItemId,
                    EventId = dto.IsEventItem ? dto.EventId : null,
                    Action = "Upload",
                    Details = $"Knowledge item '{knowledgeItem.Title}' uploaded successfully.",
                    CreatedOn = DateTime.UtcNow
                });

                // ----------------- 7️Link Knowledge Item to Event using existing team -----------------
                if (dto.IsEventItem && dto.EventId.HasValue)
                {
                    var team = await _context.Teams
                        .Where(t => t.EventId == dto.EventId.Value)
                        .Join(_context.TeamMembers, t => t.TeamId, tm => tm.TeamId, (t, tm) => new { t, tm })
                        .Where(x => x.tm.UserId == userId)
                        .Select(x => x.t)
                        .FirstOrDefaultAsync();

                    if (team == null)
                        throw new Exception("You are not registered in any team for this event.");

                    _context.EventKnowledgeItems.Add(new EventKnowledgeItem
                    {
                        EventItemId = Guid.NewGuid(),
                        EventId = dto.EventId,
                        ItemId = knowledgeItem.ItemId,
                        TeamId = team.TeamId,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedBy = userId,
                        UpdatedOn = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return knowledgeItem;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error uploading knowledge item");
                throw;
            }
        }

        #endregion

        #region Listing

        public async Task<IEnumerable<KnowledgeItemFilterDto>> GetKnowledgeItemSummariesAsync(string sortOrder = "desc", DateTime? filterDate = null)
        {
            var query = _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .AsQueryable();

            if (filterDate.HasValue)
            {
                var date = filterDate.Value.Date;
                query = query.Where(k => k.CreatedOn.HasValue &&
                                         k.CreatedOn.Value.Date == date);
            }

            query = sortOrder.ToLower() == "asc" ? query.OrderBy(k => k.CreatedOn) : query.OrderByDescending(k => k.CreatedOn);

            return await query
                .Select(k => new KnowledgeItemFilterDto
                {
                    ItemId = k.ItemId,
                    Title = k.Title,
                    Description = k.Description,
                    DomainName = k.Domain.DomainName,
                    CategoryName = k.Category.CategoryName,
                    CreatedOn = k.CreatedOn ?? DateTime.MinValue
                })
                .ToListAsync();
        }

        #region Knowledge Item Listing

        // ✅ Get all knowledge items
        public async Task<List<KnowledgeItemDto>> GetAllKnowledgeItemsAsync()
        {
            return await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.Owner)
                .Include(k => k.CreatedByNavigation)
                .Include(k => k.UpdatedByNavigation)
                .Include(k => k.KnowledgeTags)
                .Select(k => new KnowledgeItemDto
                {
                    ItemId = k.ItemId,
                    Title = k.Title,
                    Description = k.Description,
                    KnowledgeItem = k.KnowledgeText,
                    DomainId = k.DomainId,
                    DomainName = k.Domain != null ? k.Domain.DomainName : null,
                    CategoryId = k.CategoryId,
                    CategoryName = k.Category != null ? k.Category.CategoryName : null,
                    OwnerId = k.OwnerId,
                    OwnerName = k.Owner != null ? k.Owner.Name : null,
                    Status = k.Status,
                    Version = k.Version,
                    Visibility = "", // keep empty if not stored
                    IsEventItem = k.IsEventItem,
                    ContributorName = k.Owner != null ? k.Owner.Name : null,
                    CreatedBy = k.CreatedBy,
                    CreatedByName = k.CreatedByNavigation != null ? k.CreatedByNavigation.Name : null,
                    UpdatedOn = k.UpdatedOn,
                    UpdatedBy = k.UpdatedBy,
                    UpdatedByName = k.UpdatedByNavigation != null ? k.UpdatedByNavigation.Name : null,
                    CreatedOn = k.CreatedOn ?? DateTime.MinValue,
                    EngagementScore = 0, // calculate if needed
                    Metadata = k.Metadata,
                    Language = k.Language ?? "[]",
                    Framework = k.Framework ?? "[]",
                    Tags = k.KnowledgeTags != null ? k.KnowledgeTags.Select(t => t.TagName).ToList() : new List<string>(),
                    Views = 0, // set from engagement if available
                    Likes = 0,
                    Comments = 0,
                    SubmittedBy = k.Owner != null ? k.Owner.Name : string.Empty
                })
                .ToListAsync();
        }

        // ✅ Get items by domain
        public async Task<List<KnowledgeItemDto>> GetKnowledgeItemsByDomainAsync(Guid domainId)
        {
            return await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.Owner)
                .Include(k => k.CreatedByNavigation)
                .Include(k => k.UpdatedByNavigation)
                .Include(k => k.KnowledgeTags)
                .Where(k => k.DomainId == domainId)
                .Select(k => new KnowledgeItemDto
                {
                    ItemId = k.ItemId,
                    Title = k.Title,
                    Description = k.Description,
                    DomainId = k.DomainId,
                    DomainName = k.Domain != null ? k.Domain.DomainName : null,
                    CategoryId = k.CategoryId,
                    CategoryName = k.Category != null ? k.Category.CategoryName : null,
                    OwnerId = k.OwnerId,
                    OwnerName = k.Owner != null ? k.Owner.Name : null,
                    CreatedBy = k.CreatedBy,
                    CreatedByName = k.CreatedByNavigation != null ? k.CreatedByNavigation.Name : null,
                    CreatedOn = k.CreatedOn ?? DateTime.MinValue,
                    UpdatedOn = k.UpdatedOn,
                    UpdatedBy = k.UpdatedBy,
                    UpdatedByName = k.UpdatedByNavigation != null ? k.UpdatedByNavigation.Name : null,
                    EngagementScore = 0,
                    Status = k.Status,
                    Visibility = "",
                    Language = k.Language ?? "[]",
                    Framework = k.Framework ?? "[]",
                    Tags = k.KnowledgeTags != null ? k.KnowledgeTags.Select(t => t.TagName).ToList() : new List<string>(),
                    Views = 0,
                    Likes = 0,
                    Comments = 0,
                    SubmittedBy = k.Owner != null ? k.Owner.Name : string.Empty
                })
                .ToListAsync();
        }

        // ✅ Get items by category
        public async Task<List<KnowledgeItemDto>> GetKnowledgeItemsByCategoryAsync(Guid categoryId)
        {
            return await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.Owner)
                .Include(k => k.CreatedByNavigation)
                .Include(k => k.UpdatedByNavigation)
                .Include(k => k.KnowledgeTags)
                .Where(k => k.CategoryId == categoryId)
                .Select(k => new KnowledgeItemDto
                {
                    ItemId = k.ItemId,
                    Title = k.Title,
                    Description = k.Description,
                    DomainId = k.DomainId,
                    DomainName = k.Domain != null ? k.Domain.DomainName : null,
                    CategoryId = k.CategoryId,
                    CategoryName = k.Category != null ? k.Category.CategoryName : null,
                    OwnerId = k.OwnerId,
                    OwnerName = k.Owner != null ? k.Owner.Name : null,
                    CreatedBy = k.CreatedBy,
                    CreatedByName = k.CreatedByNavigation != null ? k.CreatedByNavigation.Name : null,
                    CreatedOn = k.CreatedOn ?? DateTime.MinValue,
                    UpdatedOn = k.UpdatedOn,
                    UpdatedBy = k.UpdatedBy,
                    UpdatedByName = k.UpdatedByNavigation != null ? k.UpdatedByNavigation.Name : null,
                    EngagementScore = 0,
                    Status = k.Status,
                    Visibility = "",
                    Language = k.Language ?? "[]",
                    Framework = k.Framework ?? "[]",
                    Tags = k.KnowledgeTags != null ? k.KnowledgeTags.Select(t => t.TagName).ToList() : new List<string>(),
                    Views = 0,
                    Likes = 0,
                    Comments = 0,
                    SubmittedBy = k.Owner != null ? k.Owner.Name : string.Empty
                })
                .ToListAsync();
        }

        #endregion
        #endregion

    }
}

using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Implementations
{
    public class KnowledgeItemService : IKnowledgeItemService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public KnowledgeItemService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        public async Task<KnowledgeItem> UploadKnowledgeItemAsync(KnowledgeItemUploadDto dto, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || dto.DomainId == Guid.Empty || dto.CategoryId == Guid.Empty)
                throw new ArgumentException("Title, Domain, and Category are required.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1️⃣ Create Knowledge Item
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
                    Language = JsonSerializer.Serialize(dto.Language),
                    Framework = JsonSerializer.Serialize(dto.Framework),
                    Metadata = JsonSerializer.Serialize(new { Visibility = dto.Visibility })
                };
                _context.KnowledgeItems.Add(knowledgeItem);

                // 2️⃣ Create Initial Version
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

                // 3️⃣ Add Attachments
                if (dto.Attachments?.Count > 0)
                {
                    foreach (var file in dto.Attachments)
                    {
                        _context.Attachments.Add(new Attachment
                        {
                            AttachmentId = Guid.NewGuid(),
                            ItemId = knowledgeItem.ItemId,
                            VersionId = version.VersionId,
                            FileName = file.FileName,
                            MimeType = file.MimeType,
                            FileData = file.FileData,
                            FileSize = file.FileSize,
                            CreatedOn = DateTime.UtcNow,
                            CreatedBy = userId,
                            UpdatedOn = DateTime.UtcNow,
                            UpdatedBy = userId
                        });
                    }
                }

                // 4️⃣ Add Tags (fixed)
                var tagsList = dto.Tags ?? new List<string>();

                foreach (var tag in tagsList)
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


                // 5️⃣ Event-related
                // 5️⃣ Event-related
                if (dto.IsEventItem && dto.EventId.HasValue)
                {
                    var existingEvent = await _context.Events.FindAsync(dto.EventId.Value)
                        ?? throw new Exception($"Event with Id {dto.EventId.Value} does not exist.");

                    var teamId = Guid.NewGuid();
                    var team = new Team
                    {
                        TeamId = teamId,
                        EventId = dto.EventId.Value,
                        TeamName = dto.TeamName ?? $"{dto.Title}_Team",
                        CreatedBy = userId,
                        CreatedOn = DateTime.UtcNow
                    };
                    _context.Teams.Add(team);
                    await _context.SaveChangesAsync();

                    // 6️⃣ Team Members

                    // 1️⃣ Fetch uploader automatically
                    var uploader = await _context.Users.FindAsync(userId);
                    if (uploader == null)
                        throw new Exception("Uploader not found in Users table.");

                    var emails = new List<string>();

                    // 2️⃣ Include only provided team member emails (exclude uploader)
                    if (dto.TeamMemberEmails?.Count > 0)
                    {
                        foreach (var entry in dto.TeamMemberEmails)
                        {
                            emails.AddRange(entry
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(e => e.Trim())
                                .Where(e => !string.IsNullOrEmpty(e) && e != uploader.Email)); // exclude uploader
                        }
                    }

                    // Remove duplicates
                    emails = emails.Distinct().ToList();

                    var teamMembers = new List<TeamMember>();

                    // Add team member emails
                    foreach (var email in emails)
                    {
                        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                        if (user != null)
                        {
                            teamMembers.Add(new TeamMember
                            {
                                TeamMemberId = Guid.NewGuid(),
                                TeamId = teamId,
                                UserId = user.UserId,
                                JoinedOn = DateTime.UtcNow
                            });
                        }
                    }

                    // Finally, add uploader once
                    teamMembers.Add(new TeamMember
                    {
                        TeamMemberId = Guid.NewGuid(),
                        TeamId = teamId,
                        UserId = uploader.UserId,
                        JoinedOn = DateTime.UtcNow
                    });

                    _context.TeamMembers.AddRange(teamMembers);
                    await _context.SaveChangesAsync();

                    // 7️⃣ Link Item to Event
                    var eventKnowledgeItem = new EventKnowledgeItem
                    {
                        EventItemId = Guid.NewGuid(),
                        EventId = dto.EventId.Value,
                        ItemId = knowledgeItem.ItemId,
                        TeamId = teamId,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = userId,
                        UpdatedOn = DateTime.UtcNow
                    };
                    _context.EventKnowledgeItems.Add(eventKnowledgeItem);
                    await _context.SaveChangesAsync();
                }


                // 8️⃣ Log Activity
                var activityDetails =
                    $"Knowledge item '{knowledgeItem.Title}' uploaded by user. " +
                    $"Languages: {string.Join(", ", dto.Language ?? new List<string>())}; " +
                    $"Frameworks: {string.Join(", ", dto.Framework ?? new List<string>())}; " +
                    $"Attachments: {dto.Attachments?.Count ?? 0}; " +
                    $"Tags: {string.Join(", ", tagsList)};";

                if (dto.IsEventItem)
                    activityDetails += $" Team: {dto.TeamName ?? "N/A"}; Members: {string.Join(", ", dto.TeamMemberEmails ?? new List<string>())};";

                var activity = new ActivityLog
                {
                    ActivityId = Guid.NewGuid(),
                    UserId = userId,
                    ItemId = knowledgeItem.ItemId,
                    EventId = dto.IsEventItem ? dto.EventId : null,
                    Action = "Upload Knowledge Item",
                    Details = activityDetails,
                    CreatedOn = DateTime.UtcNow
                };

                _context.ActivityLogs.Add(activity);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return knowledgeItem;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<KnowledgeItemFilterDto>> GetKnowledgeItemSummariesAsync(
     string sortOrder = "desc",
     DateTime? filterDate = null)
        {
            // Convert filterDate to UTC if it exists
            DateTime? utcFilterDate = filterDate?.Date.ToUniversalTime();

            var query = _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .AsQueryable();

            if (utcFilterDate.HasValue)
            {
                var startUtc = utcFilterDate.Value;
                var endUtc = startUtc.AddDays(1);

                query = query.Where(k => k.CreatedOn.HasValue &&
                                         k.CreatedOn.Value >= startUtc &&
                                         k.CreatedOn.Value < endUtc);
            }

            query = sortOrder.ToLower() == "asc"
                ? query.OrderBy(k => k.CreatedOn)
                : query.OrderByDescending(k => k.CreatedOn);

            var result = await query
                .Select(k => new KnowledgeItemFilterDto
                {
                    Title = k.Title,
                    DomainName = k.Domain.DomainName,
                    CategoryName = k.Category.CategoryName,
                    Description = k.Description,
                    CreatedOn = k.CreatedOn ?? DateTime.MinValue
                })
                .ToListAsync();

            return result;
        }
        // Get knowledge items by Domain
        public async Task<IEnumerable<KnowledgeItemFilterDto>> GetKnowledgeItemsByDomainAsync(Guid domainId)
        {
            var query = _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Where(k => k.DomainId == domainId);

            var result = await query
                .Select(k => new KnowledgeItemFilterDto
                {
                    Title = k.Title,
                    DomainName = k.Domain.DomainName,
                    CategoryName = k.Category.CategoryName,
                    Description = k.Description,
                    CreatedOn = k.CreatedOn ?? DateTime.MinValue
                })
                .ToListAsync();

            return result;
        }

        // Get knowledge items by Category
        public async Task<IEnumerable<KnowledgeItemFilterDto>> GetKnowledgeItemsByCategoryAsync(Guid categoryId)
        {
            var query = _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Where(k => k.CategoryId == categoryId);

            var result = await query
                .Select(k => new KnowledgeItemFilterDto
                {
                    Title = k.Title,
                    DomainName = k.Domain.DomainName,
                    CategoryName = k.Category.CategoryName,
                    Description = k.Description,
                    CreatedOn = k.CreatedOn ?? DateTime.MinValue
                })
                .ToListAsync();

            return result;
        }
        public async Task<IEnumerable<KnowledgeItemFilterDto>> GetAllKnowledgeItemsAsync()
        {
            return await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .OrderByDescending(k => k.CreatedOn)
                .Select(k => new KnowledgeItemFilterDto
                {
                    Title = k.Title,
                    Description = k.Description,
                    DomainName = k.Domain.DomainName,
                    CategoryName = k.Category.CategoryName,
                    CreatedOn = k.CreatedOn ?? DateTime.MinValue
                })
                .ToListAsync();
        }

    }
}

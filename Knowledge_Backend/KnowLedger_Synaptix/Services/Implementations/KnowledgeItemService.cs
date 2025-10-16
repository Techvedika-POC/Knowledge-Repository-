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
        //getting all details of a particular knowledge item
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
                    FileUrl = $"/attachments/{a.AttachmentId}",
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


        //uploading the knowledge items by a user
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
                    KnowledgeItem1 = dto.Description ?? "",  
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

                //  Create Initial Version
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
                var attachmentsList = new List<Attachment>();

             
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder); // Make sure the folder exists
                }

                foreach (var file in dto.Attachments)
                {
                    var attachmentId = Guid.NewGuid();
                    var filePath = Path.Combine(uploadsFolder, file.FileName); // Save file to the uploads folder
                    await File.WriteAllBytesAsync(filePath, file.FileData); // Save metadata to the database

                    var attachment = new Attachment
                    {
                        AttachmentId = attachmentId,
                        ItemId = knowledgeItem.ItemId,
                        VersionId = version.VersionId,
                        FileName = file.FileName,
                        FilePath = $"/uploads/{file.FileName}", // This is the URL path
                        FileData = file.FileData, // optional: you may skip storing raw data in DB if stored on disk
                        MimeType = file.MimeType,
                        FileSize = file.FileSize,
                        FileType = file.MimeType,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = userId,
                        UpdatedOn = DateTime.UtcNow,
                        UpdatedBy = userId
                    };

                    _context.Attachments.Add(attachment);
                }

                await _context.SaveChangesAsync();

                // Add Tags
                var tagsList = (dto.Tags ?? new List<string>())
                    .Select(tag => new KnowledgeTag
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
                _context.KnowledgeTags.AddRange(tagsList);

                // Event-related 
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

                    var uploader = await _context.Users.FindAsync(userId)
                        ?? throw new Exception("Uploader not found in Users table.");

                    // Add team members
                    var emails = dto.TeamMemberEmails?
                        .SelectMany(e => e.Split(',', StringSplitOptions.RemoveEmptyEntries))
                        .Select(e => e.Trim())
                        .Where(e => !string.IsNullOrEmpty(e) && e != uploader.Email)
                        .Distinct()
                        .ToList() ?? new List<string>();

                    var teamMembers = new List<TeamMember>();
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

                    // Add the team member who is uploading
                    teamMembers.Add(new TeamMember
                    {
                        TeamMemberId = Guid.NewGuid(),
                        TeamId = teamId,
                        UserId = uploader.UserId,
                        JoinedOn = DateTime.UtcNow
                    });
                    _context.TeamMembers.AddRange(teamMembers);

                    // Link item to event
                    _context.EventKnowledgeItems.Add(new EventKnowledgeItem
                    {
                        EventItemId = Guid.NewGuid(),
                        EventId = dto.EventId.Value,
                        ItemId = knowledgeItem.ItemId,
                        TeamId = teamId,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = userId,
                        UpdatedOn = DateTime.UtcNow
                    });
                }

                // 6️ Activity Log
                var activityDetails =
                    $"Knowledge item '{knowledgeItem.Title}' uploaded by user. " +
                    $"Languages: {string.Join(", ", dto.Language ?? new List<string>())}; " +
                    $"Frameworks: {string.Join(", ", dto.Framework ?? new List<string>())}; " +
                    $"Attachments: {attachmentsList.Count}; " +
                    $"Tags: {string.Join(", ", tagsList.Select(t => t.TagName))};";

                if (dto.IsEventItem)
                    activityDetails += $" Team: {dto.TeamName ?? "N/A"}; Members: {string.Join(", ", dto.TeamMemberEmails ?? new List<string>())};";

                _context.ActivityLogs.Add(new ActivityLog
                {
                    ActivityId = Guid.NewGuid(),
                    UserId = userId,
                    ItemId = knowledgeItem.ItemId,
                    EventId = dto.IsEventItem ? dto.EventId : null,
                    Action = "Upload Knowledge Item",
                    Details = activityDetails,
                    CreatedOn = DateTime.UtcNow
                });

                // Save everything in one transaction**
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return knowledgeItem;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Log exception
                Console.WriteLine(ex);
                throw;
            }
        }

        //get knowldege items by datewise realted to the user
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
                .Include(k => k.Owner) // include the owner user
                .Where(k => k.DomainId == domainId);

            var result = await query
                .Select(k => new KnowledgeItemFilterDto
                {
                    Title = k.Title,
                    DomainName = k.Domain.DomainName,
                    CategoryName = k.Category.CategoryName,
                    Description = k.Description,
                    SubmittedBy = k.Owner != null ? k.Owner.Name : "Unknown", // get owner name
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
                .Include(k => k.Owner) // include the owner user
                .Where(k => k.CategoryId == categoryId);

            var result = await query
                .Select(k => new KnowledgeItemFilterDto
                {
                    Title = k.Title,
                    DomainName = k.Domain.DomainName,
                    CategoryName = k.Category.CategoryName,
                    Description = k.Description,
                    SubmittedBy = k.Owner != null ? k.Owner.Name : "Unknown", // get owner name
                    CreatedOn = k.CreatedOn ?? DateTime.MinValue
                })
                .ToListAsync();

            return result;
        }
        //Getting knowledge items of the user
        public async Task<IEnumerable<KnowledgeItemFilterDto>> GetAllKnowledgeItemsAsync()
        {
            return await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                 .Include(k => k.Owner)
                .OrderByDescending(k => k.CreatedOn)
                .Select(k => new KnowledgeItemFilterDto
                {
                    Title = k.Title,
                    Description = k.Description,
                    
                    SubmittedBy = k.Owner != null ? k.Owner.Name : "Unknown", // get owner name
                    DomainName = k.Domain.DomainName,
                    CategoryName = k.Category.CategoryName,
                    CreatedOn = k.CreatedOn ?? DateTime.MinValue
                })
                .ToListAsync();
        }

    }
}

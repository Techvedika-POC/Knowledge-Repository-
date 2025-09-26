using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
namespace KnowLedger_Synaptix.Services.Implementations
{
    public class KnowledgeItemService : IKnowledgeItemService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public KnowledgeItemService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        public async Task<KnowledgeItem> UploadKnowledgeItemAsync(
            KnowledgeItemUploadDto dto,
            Guid userId
        )
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || dto.DomainId == Guid.Empty || dto.CategoryId == Guid.Empty)
                throw new ArgumentException("Title, Domain, and Category are required.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ===== 1. Knowledge Item =====
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
                    IsEventItem = dto.IsEventItem,

                    Language = JsonSerializer.Serialize(dto.Language),
                    Framework = JsonSerializer.Serialize(dto.Framework),
                    Metadata = System.Text.Json.JsonSerializer.Serialize(new { dto.Visibility }),
                    

                };
                _context.KnowledgeItems.Add(knowledgeItem);

                // ===== 2. Knowledge Version =====
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

                // ===== 3. Attachments =====
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
                            UpdatedBy = userId,


                        });
                    }
                }

                // ===== 4. Tags =====
                if (dto.Tags?.Count > 0)
                {
                    foreach (var tag in dto.Tags)
                    {
                        _context.KnowledgeTags.Add(new KnowledgeTag
                        {
                            TagId = Guid.NewGuid(),
                            ItemId = knowledgeItem.ItemId,
                            VersionId = version.VersionId,
                            TagName = tag,
                            CreatedOn = DateTime.UtcNow,
                            CreatedBy = userId,
                              UpdatedOn = DateTime.UtcNow,
                            UpdatedBy = userId,


                        });
                    }
                }

                await _context.SaveChangesAsync();

                // ===== 5. Event-specific inserts =====
                if (dto.IsEventItem && dto.EventId.HasValue)
                {
                    // Ensure the Event exists
                    var existingEvent = await _context.Events.FindAsync(dto.EventId.Value);
                    if (existingEvent == null)
                        throw new Exception($"Event with Id {dto.EventId.Value} does not exist.");

                    // Ensure the Owner/User exists
                    var owner = await _context.Users.FindAsync(userId);
                    if (owner == null)
                        throw new Exception($"User with Id {userId} does not exist.");

                    // Create a Team
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

                    // ===== 6. Add Team Members =====
                    var teamMembers = new List<TeamMember>();

                    var emails = new List<string>();

                    // Split each input in TeamMemberEmails by comma
                    if (dto.TeamMemberEmails?.Count > 0)
                    {
                        foreach (var emailEntry in dto.TeamMemberEmails)
                        {
                            emails.AddRange(emailEntry
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(e => e.Trim()));
                        }
                    }

                    // Remove duplicates and empty entries
                    emails = emails.Where(e => !string.IsNullOrEmpty(e))
                                   .Distinct()
                                   .ToList();

                    if (emails.Count > 0)
                    {
                        var users = await _context.Users
                            .Where(u => emails.Contains(u.Email))
                            .ToListAsync();

                        foreach (var member in users)
                        {
                            teamMembers.Add(new TeamMember
                            {
                                TeamMemberId = Guid.NewGuid(),
                                TeamId = teamId,
                                UserId = member.UserId,
                                JoinedOn = DateTime.UtcNow
                            });
                        }
                    }

                    // If no valid emails, add owner by default
                    if (teamMembers.Count == 0)
                    {
                        teamMembers.Add(new TeamMember
                        {
                            TeamMemberId = Guid.NewGuid(),
                            TeamId = teamId,
                            UserId = userId,
                            JoinedOn = DateTime.UtcNow
                        });
                    }

                    _context.TeamMembers.AddRange(teamMembers);
                    await _context.SaveChangesAsync();

                    // ===== 7. Link KnowledgeItem to Event =====
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

                await transaction.CommitAsync();
                return knowledgeItem;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
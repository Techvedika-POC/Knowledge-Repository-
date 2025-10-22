using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;
using KnowledgeSynaptix.Services.Implementations;
using KnowledgeSynaptix.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Implementations
{
    /// <summary>
    /// Service responsible for all Knowledge Item operations.
    /// This includes creation, retrieval, filtering, attachments handling,
    /// embedding generation, and storage in the Qdrant vector database.
    /// </summary>
    public class KnowledgeItemService : IKnowledgeItemService
    {
        private readonly Knowledge_Repository_dbContext _context;
        private readonly IEmbeddingService _embeddingService;
        private readonly HttpClient _qdrantClient;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<KnowledgeItemService> _logger;
        private readonly IFileEmbeddingService _fileEmbeddingService;
        private readonly IQdrantService _qdrantService;

        /// <summary>
        /// Constructor for dependency injection.
        /// Initializes the database context, embedding services, Qdrant service, hosting environment, and logger.
        /// Throws ArgumentNullException if any required dependency is null.
        /// </summary>
        public KnowledgeItemService(
            Knowledge_Repository_dbContext context,
            IEmbeddingService embeddingService,
            IFileEmbeddingService fileEmbeddingService,
            IQdrantService qdrantService,
            IWebHostEnvironment env,
            ILogger<KnowledgeItemService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
            _fileEmbeddingService = fileEmbeddingService ?? throw new ArgumentNullException(nameof(fileEmbeddingService));
            _qdrantService = qdrantService ?? throw new ArgumentNullException(nameof(qdrantService));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Knowledge Item Retrieval

        /// <summary>
        /// Retrieves full details for a single knowledge item, including tags, attachments, owner, category, and domain.
        /// Returns null if the item does not exist.
        /// </summary>
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

            // Map database entity to DTO for API consumption
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

        /// <summary>
        /// Handles uploading a new knowledge item.
        /// 1. Creates the KnowledgeItem entity
        /// 2. Creates an initial version
        /// 3. Saves attachments and generates embeddings
        /// 4. Adds tags
        /// 5. Generates the text embedding and stores it in Qdrant
        /// The method runs inside a database transaction to ensure consistency.
        /// </summary>
        public async Task<KnowledgeItem> UploadKnowledgeItemAsync(KnowledgeItemUploadDto dto, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || dto.DomainId == Guid.Empty || dto.CategoryId == Guid.Empty)
                throw new ArgumentException("Title, Domain, and Category are required.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                //  Create Knowledge Item
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
                await _context.SaveChangesAsync();

                //  Handle Attachments
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

                            _context.Attachments.Add(new Attachment
                            {
                                AttachmentId = Guid.NewGuid(),
                                ItemId = knowledgeItem.ItemId,
                                VersionId = version.VersionId,
                                FileName = file.FileName,
                                FilePath = $"/uploads/{uniqueFileName}",
                                MimeType = file.MimeType,
                                FileSize = file.FileSize,
                                FileData = null,
                                FileType = Path.GetExtension(file.FileName)?.ToLower() ?? "unknown",
                                CreatedOn = DateTime.UtcNow,
                                CreatedBy = userId,
                                UpdatedOn = DateTime.UtcNow,
                                UpdatedBy = userId
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                //  Add Tags
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

                //  Handle Event & Team (if applicable)
                if (dto.IsEventItem && dto.EventId.HasValue)
                {
                    // Create Team
                    var team = new Team
                    {
                        TeamId = Guid.NewGuid(),
                        EventId = dto.EventId,
                        TeamName = dto.TeamName ?? "Default Team",
                        CreatedBy = userId,
                        CreatedOn = DateTime.UtcNow
                    };
                    _context.Teams.Add(team);
                    await _context.SaveChangesAsync();

                    // Add Team Members
                    if (dto.TeamMemberEmails != null)
                    {
                        foreach (var email in dto.TeamMemberEmails)
                        {
                            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                            if (user != null)
                            {
                                _context.TeamMembers.Add(new TeamMember
                                {
                                    TeamMemberId = Guid.NewGuid(),
                                    TeamId = team.TeamId,
                                    UserId = user.UserId,
                                    JoinedOn = DateTime.UtcNow
                                });
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    // Create EventKnowledgeItem
                    var eventKnowledgeItem = new EventKnowledgeItem
                    {
                        EventItemId = Guid.NewGuid(),
                        EventId = dto.EventId,
                        ItemId = knowledgeItem.ItemId,
                        TeamId = team.TeamId,
                        CreatedBy = userId,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedBy = userId,
                        UpdatedOn = DateTime.UtcNow
                    };
                    _context.EventKnowledgeItems.Add(eventKnowledgeItem);
                    await _context.SaveChangesAsync();
                }

                //  Create Activity Log for Upload (Action as string)
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

        #endregion

        #region Knowledge Item Listing & Filtering

        /// <summary>
        /// Retrieves a list of knowledge items with optional filtering by date and sorting.
        /// Returns summarized DTOs for listing in UI or API endpoints.
        /// </summary>
        public async Task<IEnumerable<KnowledgeItemFilterDto>> GetKnowledgeItemSummariesAsync(string sortOrder = "desc", DateTime? filterDate = null)
        {
            DateTime? utcFilterDate = filterDate?.Date.ToUniversalTime();
            var query = _context.KnowledgeItems.Include(k => k.Domain).Include(k => k.Category).AsQueryable();

            if (utcFilterDate.HasValue)
            {
                var startUtc = utcFilterDate.Value;
                var endUtc = startUtc.AddDays(1);
                query = query.Where(k => k.CreatedOn.HasValue && k.CreatedOn.Value >= startUtc && k.CreatedOn.Value < endUtc);
            }

            query = sortOrder.ToLower() == "asc" ? query.OrderBy(k => k.CreatedOn) : query.OrderByDescending(k => k.CreatedOn);

            return await query
                .Select(k => new KnowledgeItemFilterDto
                {
                    Title = k.Title,
                    DomainName = k.Domain.DomainName,

                    CategoryName = k.Category.CategoryName,
                    Description = k.Description,
                    CreatedOn = k.CreatedOn ?? DateTime.MinValue
                })
                .ToListAsync();
        }
        public async Task<IEnumerable<KnowledgeItemFilterDto>> GetKnowledgeItemsByDomainAsync(Guid domainId)
        {
            return await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.Owner) 
                .Where(k => k.DomainId == domainId)
                .Select(k => new KnowledgeItemFilterDto
                {
                    ItemId = k.ItemId,
                    Title = k.Title,
                    Description = k.Description,
                    DomainName = k.Domain.DomainName,
                    CategoryName = k.Category.CategoryName,
                    SubmittedBy = k.Owner != null ? k.Owner.Name : "Unknown", 
                    Status = k.Status ?? string.Empty,
                    CreatedOn = k.CreatedOn ?? DateTime.MinValue,
                    Tags = new List<string>() 
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<KnowledgeItemFilterDto>> GetKnowledgeItemsByCategoryAsync(Guid categoryId)
        {
            return await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.Owner)
                .Where(k => k.CategoryId == categoryId)
                .Select(k => new KnowledgeItemFilterDto
                {
                    ItemId = k.ItemId,
                    Title = k.Title,
                    Description = k.Description,
                    DomainName = k.Domain.DomainName,
                    CategoryName = k.Category.CategoryName,
                    SubmittedBy = k.Owner != null ? k.Owner.Name : "Unknown",
                    Status = k.Status ?? string.Empty,
                    CreatedOn = k.CreatedOn ?? DateTime.MinValue,
                    Tags = new List<string>()
                })
                .ToListAsync();
        }


        /// <summary>
        /// Retrieves all knowledge items, sorted by creation date (newest first),
        /// including domain, category, and submitter details.
        /// </summary>
        public async Task<IEnumerable<KnowledgeItemFilterDto>> GetAllKnowledgeItemsAsync()
        {
            return await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.Owner) 
                .OrderByDescending(k => k.CreatedOn)
                .Select(k => new KnowledgeItemFilterDto
                {
                    ItemId = k.ItemId, 
                    Title = k.Title,
                    Description = k.Description,
                    DomainName = k.Domain != null ? k.Domain.DomainName : "Unknown Domain",
                    CategoryName = k.Category != null ? k.Category.CategoryName : "Unknown Category",
                    SubmittedBy = k.Owner != null ? k.Owner.Name : "Unknown", 
                    Status = k.Status ?? string.Empty,
                    CreatedOn = k.CreatedOn ?? DateTime.MinValue,
                    Tags = new List<string>() 
                })
                .ToListAsync();
        }

        #endregion
    }
}
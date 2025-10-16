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

            const int ExpectedEmbeddingSize = 384; // Ensure embedding dimension matches Qdrant collection

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ----------------- 1️⃣ Create Knowledge Item -----------------
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

                // ----------------- 2️⃣ Create Initial Version -----------------
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

                // ----------------- 3️⃣ Handle Attachments & Embeddings -----------------
                var uploadsRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot"), "uploads");
                Directory.CreateDirectory(uploadsRoot);

                if (dto.Attachments?.Count > 0)
                {
                    foreach (var file in dto.Attachments)
                    {
                        try
                        {
                            if (file.FileData == null || file.FileData.Length == 0)
                            {
                                _logger.LogWarning("Attachment {FileName} has no data. Only metadata will be saved.", file.FileName);
                            }

                            const long MaxBytes = 100 * 1024 * 1024; // 100 MB limit
                            if (file.FileSize > MaxBytes)
                            {
                                _logger.LogWarning("Attachment {FileName} exceeds max allowed size.", file.FileName);
                                continue;
                            }

                            string? diskPath = null;
                            if (file.FileData?.Length > 0)
                            {
                                var safeName = Path.GetFileName(file.FileName);
                                var uniqueFileName = $"{Guid.NewGuid()}_{safeName}";
                                diskPath = Path.Combine(uploadsRoot, uniqueFileName);
                                await File.WriteAllBytesAsync(diskPath, file.FileData);
                            }

                            // Save attachment metadata
                            var attachment = new Attachment
                            {
                                AttachmentId = Guid.NewGuid(),
                                ItemId = knowledgeItem.ItemId,
                                VersionId = version.VersionId,
                                FileName = file.FileName,
                                FilePath = diskPath != null ? $"/uploads/{Path.GetFileName(diskPath)}" : null,
                                MimeType = file.MimeType,
                                FileSize = file.FileSize,
                                FileData = null,
                                CreatedOn = DateTime.UtcNow,
                                CreatedBy = userId,
                                UpdatedOn = DateTime.UtcNow,
                                UpdatedBy = userId
                            };
                            _context.Attachments.Add(attachment);
                            await _context.SaveChangesAsync();

                            // Generate embeddings for text-based files
                            if (diskPath != null && !file.MimeType.StartsWith("audio") && !file.MimeType.StartsWith("video"))
                            {
                                var fileEmbedding = await _fileEmbeddingService.GenerateEmbeddingAsync(diskPath, file.MimeType);

                                if (fileEmbedding == null || fileEmbedding.Count != ExpectedEmbeddingSize)
                                {
                                    _logger.LogWarning(
                                        "Attachment embedding for {FileName} is invalid length: {Length}",
                                        file.FileName,
                                        fileEmbedding?.Count ?? 0
                                    );
                                }
                                else
                                {
                                    await _qdrantService.SaveToQdrantAsync(
                                        attachment.AttachmentId.ToString(),
                                        fileEmbedding.ToArray(),
                                        file.FileName,
                                        knowledgeItem.Title
                                    );
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to process attachment {FileName}", file.FileName);
                        }
                    }
                }

                // ----------------- 4️⃣ Add Tags -----------------
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

                // ----------------- 5️⃣ Generate Knowledge Item Text Embedding -----------------
                try
                {
                    var textEmbedding = await _embeddingService.GetEmbeddingAsync(knowledgeItem.Description ?? knowledgeItem.Title ?? "");

                    if (textEmbedding == null)
                    {
                        _logger.LogError("No embedding returned for knowledge item {ItemId}", knowledgeItem.ItemId);
                        throw new InvalidOperationException("Failed to generate embedding.");
                    }

                    if (textEmbedding.Length != ExpectedEmbeddingSize)
                    {
                        _logger.LogError("Embedding has invalid dimension {Dim} for item {ItemId}", textEmbedding.Length, knowledgeItem.ItemId);
                        throw new InvalidOperationException($"Embedding must be {ExpectedEmbeddingSize}-dimensional.");
                    }

                    knowledgeItem.Embedding = textEmbedding.ToList();

                    await _qdrantService.SaveToQdrantAsync(
                        knowledgeItem.ItemId.ToString(),
                        knowledgeItem.Embedding.ToArray(),
                        knowledgeItem.Title,
                        knowledgeItem.Description ?? ""
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Embedding generation failed for knowledge item {ItemId}", knowledgeItem.ItemId);
                }

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

        /// <summary>
        /// Retrieves knowledge items for a specific domain.
        /// </summary>
        public async Task<IEnumerable<KnowledgeItemFilterDto>> GetKnowledgeItemsByDomainAsync(Guid domainId)
        {
            return await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Where(k => k.DomainId == domainId)
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

        /// <summary>
        /// Retrieves knowledge items for a specific category.
        /// </summary>
        public async Task<IEnumerable<KnowledgeItemFilterDto>> GetKnowledgeItemsByCategoryAsync(Guid categoryId)
        {
            return await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Where(k => k.CategoryId == categoryId)
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

        /// <summary>
        /// Retrieves all knowledge items, sorted by creation date descending.
        /// </summary>
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

        #endregion
    }
}

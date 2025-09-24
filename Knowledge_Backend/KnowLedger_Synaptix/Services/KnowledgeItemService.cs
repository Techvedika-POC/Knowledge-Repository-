using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services
{
    public class KnowledgeItemService : IKnowledgeItemService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public KnowledgeItemService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        public async Task<KnowledgeItemResponseDto> UploadKnowledgeItemAsync(KnowledgeItemUploadDto dto, Guid userId)
        {
            if (dto.DomainId.HasValue && !await _context.Domains.AnyAsync(d => d.DomainId == dto.DomainId.Value))
                throw new ArgumentException("Invalid DomainId provided.");

            if (dto.CategoryId.HasValue && !await _context.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId.Value))
                throw new ArgumentException("Invalid CategoryId provided.");

            if (dto.IsEventRelated && dto.EventId.HasValue &&
                !await _context.Events.AnyAsync(e => e.EventId == dto.EventId.Value))
                throw new ArgumentException("Invalid EventId provided.");

            var entity = new KnowledgeItem
            {
                ItemId = Guid.NewGuid(),
                Title = dto.Title ?? string.Empty,
                Description = dto.Description ?? string.Empty,
                DomainId = dto.DomainId,
                CategoryId = dto.CategoryId,
                Language = dto.Language ?? string.Empty,
                Framework = ConvertToJsonArray(dto.Frameworks),
                Metadata = ConvertToJsonArray(dto.Tags),
                CreatedOn = DateTime.UtcNow,
                CreatedBy = userId,
                Attachments = new List<Attachment>(),
                EventKnowledgeItems = new List<EventKnowledgeItem>()
            };

            if (dto.Attachments?.Any() == true)
            {
                foreach (var att in dto.Attachments)
                {
                    entity.Attachments.Add(new Attachment
                    {
                        AttachmentId = Guid.NewGuid(),
                        FileName = att.FileName ?? "Unnamed",
                        MimeType = att.MimeType ?? "application/octet-stream",
                        FileData = att.FileData,
                        FileSize = att.FileSize,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = userId
                    });
                }
            }

            if (dto.IsEventRelated && dto.EventId.HasValue)
            {
                entity.IsEventItem = true;
                entity.EventKnowledgeItems.Add(new EventKnowledgeItem
                {
                    EventItemId = Guid.NewGuid(),
                    EventId = dto.EventId.Value,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = userId
                });
            }

            _context.KnowledgeItems.Add(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Error saving KnowledgeItem. Check related foreign keys.", ex);
            }

            return MapToDto(entity);
        }

        public async Task<IEnumerable<KnowledgeItemResponseDto>> GetAllAsync()
        {
            var items = await _context.KnowledgeItems
                .Include(k => k.EventKnowledgeItems)
                .AsNoTracking()
                .ToListAsync();

            return items.Select(MapToDto);
        }

        public async Task<KnowledgeItemResponseDto?> GetByIdAsync(Guid itemId)
        {
            var entity = await _context.KnowledgeItems
                .Include(k => k.EventKnowledgeItems)
                .FirstOrDefaultAsync(k => k.ItemId == itemId);

            return entity == null ? null : MapToDto(entity);
        }

        public async Task<KnowledgeItemResponseDto?> UpdateAsync(Guid itemId, KnowledgeItemUploadDto dto)
        {
            var entity = await _context.KnowledgeItems.FindAsync(itemId);
            if (entity == null) return null;

            if (dto.Title != null) entity.Title = dto.Title;
            if (dto.Description != null) entity.Description = dto.Description;
            if (dto.DomainId.HasValue) entity.DomainId = dto.DomainId;
            if (dto.CategoryId.HasValue) entity.CategoryId = dto.CategoryId;
            if (dto.Language != null) entity.Language = dto.Language;
            if (dto.Frameworks != null) entity.Framework = ConvertToJsonArray(dto.Frameworks);
            if (dto.Tags != null) entity.Metadata = ConvertToJsonArray(dto.Tags);

            entity.UpdatedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(entity);
        }

        public async Task<bool> DeleteAsync(Guid itemId)
        {
            var entity = await _context.KnowledgeItems.FindAsync(itemId);
            if (entity == null) return false;

            _context.KnowledgeItems.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Domain>> GetAllDomainsAsync() =>
            await _context.Domains.AsNoTracking().ToListAsync();

        public async Task<IEnumerable<Category>> GetCategoriesByDomainAsync(Guid domainId) =>
            await _context.Categories.Where(c => c.DomainId == domainId).AsNoTracking().ToListAsync();

        public async Task<IEnumerable<Event>> GetAllEventsAsync() =>
            await _context.Events.AsNoTracking().ToListAsync();

        private static KnowledgeItemResponseDto MapToDto(KnowledgeItem entity) => new()
        {
            ItemId = entity.ItemId,
            Title = entity.Title,
            Description = entity.Description,
            DomainId = entity.DomainId,
            CategoryId = entity.CategoryId,
            Language = entity.Language,
            Framework = entity.Framework,
            Metadata = entity.Metadata,
            IsEventRelated = entity.IsEventItem ?? false,
            EventId = entity.EventKnowledgeItems.FirstOrDefault()?.EventId,
            CreatedOn = entity.CreatedOn ?? DateTime.MinValue
        };

        private static string ConvertToJsonArray(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "[]";

            var items = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var jsonItems = string.Join(",", items.Select(i => $"\"{i.Replace("\"", "\\\"")}\""));
            return $"[{jsonItems}]";
        }
    }
}

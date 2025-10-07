using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowLedger_Synaptix.Services.Implementations
{
    public class FreshPickService : IFreshPickService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public FreshPickService(Knowledge_Repository_dbContext  context)
        {
            _context = context;
        }

        public async Task<List<FreshPickDto>> GetFreshPicksAsync(int count = 10)
        {
            var items = await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.Owner)
                .Include(k => k.KnowledgeTags)
                .OrderByDescending(k => k.CreatedOn)
                .Take(count)
                .ToListAsync();

            return items.Select(k => new FreshPickDto
            {
                ItemId = k.ItemId,
                Title = k.Title,
                Description = k.Description,
                DomainName = k.Domain?.DomainName,
                CategoryName = k.Category?.CategoryName,
                OwnerName = k.Owner?.Name, // make sure User has Name property
                CreatedOn = k.CreatedOn ?? DateTime.MinValue,
                Tags = k.KnowledgeTags.Select(t => t.TagName).ToList()
            }).ToList();
        }
    }
}

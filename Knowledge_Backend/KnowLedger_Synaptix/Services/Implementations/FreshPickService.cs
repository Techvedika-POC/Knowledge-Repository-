using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KnowLedger_Synaptix.Services.Implementations
{
    public class FreshPickService : IFreshPickService
    {
        private readonly Knowledge_Repository_dbContext _context;

        public FreshPickService(Knowledge_Repository_dbContext context)
        {
            _context = context;
        }

        //Get items that are recently uploaded
        public async Task<List<KnowledgeItemFilterDto>> GetFreshPicksAsync(int count = 10)
        {
            var items = await _context.KnowledgeItems
                .Include(k => k.Domain)
                .Include(k => k.Category)
                .Include(k => k.Owner) 
                .Include(k => k.KnowledgeTags)
                .OrderByDescending(k => k.CreatedOn)
                .Take(count)
                .ToListAsync();

            return items.Select(k => new KnowledgeItemFilterDto
            {
                ItemId = k.ItemId,
                Title = k.Title,
                Description = k.Description,
                DomainName = k.Domain?.DomainName,
                CategoryName = k.Category?.CategoryName,
                SubmittedBy = k.Owner?.Name,
                CreatedOn = k.CreatedOn ?? DateTime.MinValue,
                Tags = k.KnowledgeTags.Select(t => t.TagName).ToList(),
                User = k.Owner 
            }).ToList();
        }
    }
}

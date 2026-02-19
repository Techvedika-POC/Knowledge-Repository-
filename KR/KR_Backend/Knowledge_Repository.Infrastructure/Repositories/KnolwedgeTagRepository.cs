using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class KnowledgeTagRepository : GenericRepository<KnowledgeTag>, IKnowledgeTagRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public KnowledgeTagRepository(Knowledge_Repository_dbContext context) : base(context)
        {
            _context = context;
        }
        public async Task AddRangeAsync(IEnumerable<KnowledgeTag> tags)
        {
            await _context.KnowledgeTags.AddRangeAsync(tags);
            await _context.SaveChangesAsync();
        }
        public async Task<List<KnowledgeTag>> GetTagsByItemIdAsync(Guid itemId)
        {
            return await _context.KnowledgeTags
                                 .Where(t => t.ItemId == itemId)
                                 .ToListAsync();
        }
        public async Task<List<KnowledgeTag>> GetByItemAndVersionAsync(
    Guid itemId,
    Guid versionId)
        {
            return await _context.KnowledgeTags
                .Where(t => t.ItemId == itemId && t.VersionId == versionId)
                .ToListAsync();
        }

    }
}
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
    public class KnowledgeVersionRepository : GenericRepository<KnowledgeVersion>, IKnowledgeVersionRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public KnowledgeVersionRepository(Knowledge_Repository_dbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<KnowledgeVersion?> GetLastVersionByItemIdAsync(Guid itemId)
        {
            return await _context.KnowledgeVersions
                .Where(v => v.ItemId == itemId)
                .OrderByDescending(v => v.VersionNumber)
                .FirstOrDefaultAsync();
        }


    }
}

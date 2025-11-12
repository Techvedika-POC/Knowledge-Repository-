using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class DomainRepository : GenericRepository<Domains>, IDomainRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public DomainRepository(Knowledge_Repository_dbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Domains?> GetByNameAsync(string domainName)
        {
            return await _context.Domains
                .Include(d => d.Categories)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DomainName.ToLower() == domainName.ToLower());
        }

        public async Task<List<Category>> GetCategoriesByDomainIdAsync(Guid domainId)
        {
            return await _context.Categories
                .Where(c => c.DomainId == domainId)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}

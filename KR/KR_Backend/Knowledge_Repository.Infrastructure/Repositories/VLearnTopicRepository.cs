using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class VLearnTopicRepository : GenericRepository<Topic>, IVLearnTopicRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public VLearnTopicRepository(Knowledge_Repository_dbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Topic>> GetAllTopicsAsync()
        {
            return await _context.Topics
                .AsNoTracking()
                .ToListAsync();
        }
    }
}

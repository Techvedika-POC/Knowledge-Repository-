using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class DaySpotlightRepository : GenericRepository<SpotlightItem>, IDaySpotlightRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public DaySpotlightRepository(Knowledge_Repository_dbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<SpotlightItem?> GetLatestSpotlightAsync()
        {
            // Efficient query — single record, no tracking, ordered by CreatedOn DESC
            return await _context.SpotlightItems
                .AsNoTracking()
                .OrderByDescending(s => s.CreatedOn)
                .FirstOrDefaultAsync();
        }
    }
}

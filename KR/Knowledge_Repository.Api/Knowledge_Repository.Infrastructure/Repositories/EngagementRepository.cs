using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Knowledge_Repository.Infrastructure.Repositories
{
    public class EngagementRepository : GenericRepository<Engagement>, IEngagementRepository
    {
        private readonly Knowledge_Repository_dbContext _context;

        public EngagementRepository(Knowledge_Repository_dbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<int> GetLikesCountAsync(Guid itemId)
        {
            return await _context.Engagements
                .CountAsync(e => e.ItemId == itemId && e.EngagementType == "Like");
        }

        public async Task<List<Engagement>> GetUserEngagementsAsync(Guid userId)
        {
            return await _context.Engagements
                .Where(e => e.UserId == userId)
                .Include(e => e.Item)
                    .ThenInclude(i => i.Domain)
                .Include(e => e.Item)
                    .ThenInclude(i => i.Category)
                .ToListAsync();
        }

        public async Task<List<Engagement>> GetFavouritesByUserAsync(Guid userId)
        {
            return await _context.Engagements
                .Where(e => e.UserId == userId && e.EngagementType == "Favourite")
                .Include(e => e.Item)
                    .ThenInclude(i => i.Domain)
                .Include(e => e.Item)
                    .ThenInclude(i => i.Category)
                .ToListAsync();
        }

        /// <summary>
        /// EF Core-compatible GroupBy for top liked items
        /// </summary>
        public async Task<List<TResult>> GroupByAsync<TKey, TResult>(
            Expression<Func<Engagement, bool>> filter,
            Expression<Func<Engagement, TKey>> keySelector,
            Expression<Func<IGrouping<TKey, Engagement>, TResult>> resultSelector)
        {
            return await _context.Engagements
                .Where(filter)
                .AsNoTracking()
                .GroupBy(keySelector)
                .Select(resultSelector)
                .ToListAsync();
        }
    }
}

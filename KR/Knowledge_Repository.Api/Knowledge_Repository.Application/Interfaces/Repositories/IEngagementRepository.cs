using Knowledge_Repository.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IEngagementRepository : IGenericRepository<Engagement>
    {
        Task<int> GetLikesCountAsync(Guid itemId);
        Task<List<Engagement>> GetUserEngagementsAsync(Guid userId);
        Task<List<Engagement>> GetFavouritesByUserAsync(Guid userId);

        // EF Core-compatible grouping for top liked items
        Task<List<TResult>> GroupByAsync<TKey, TResult>(
            Expression<Func<Engagement, bool>> filter,
            Expression<Func<Engagement, TKey>> keySelector,
            Expression<Func<IGrouping<TKey, Engagement>, TResult>> resultSelector);
    }
}

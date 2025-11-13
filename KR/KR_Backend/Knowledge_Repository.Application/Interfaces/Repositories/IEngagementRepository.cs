using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Domain.Entities;
using System.Linq.Expressions;

public interface IEngagementRepository : IGenericRepository<Engagement>
{
    Task<int> GetLikesCountAsync(Guid itemId);
    Task<List<Engagement>> GetUserEngagementsAsync(Guid userId);
    Task<List<Engagement>> GetFavouritesByUserAsync(Guid userId);
    Task<List<TResult>> GroupByAsync<TKey, TResult>(
        Expression<Func<Engagement, bool>> filter,
        Expression<Func<Engagement, TKey>> keySelector,
        Expression<Func<IGrouping<TKey, Engagement>, TResult>> resultSelector);
    Task<List<Engagement>> GetCommentsByItemAsync(Guid itemId);
    Task<List<Engagement>> GetRepliesAsync(Guid parentCommentId);
}

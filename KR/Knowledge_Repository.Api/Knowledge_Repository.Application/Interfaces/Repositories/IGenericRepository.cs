using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        Task<List<TResult>> GroupByAsync<TKey, TResult>(
            Expression<Func<T, TKey>> keySelector,
            Expression<Func<IGrouping<TKey, T>, TResult>> resultSelector);

        Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<Guid> ids);
        Task AddRangeAsync(IEnumerable<T> entities);
        IQueryable<T> Query();
    }
}

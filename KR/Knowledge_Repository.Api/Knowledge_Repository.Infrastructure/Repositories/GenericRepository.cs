using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly Knowledge_Repository_dbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(Knowledge_Repository_dbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);
    public virtual async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.AsNoTracking().ToListAsync();
    public virtual async Task AddAsync(T entity) { await _dbSet.AddAsync(entity); await _context.SaveChangesAsync(); }
    public virtual async Task UpdateAsync(T entity) { _dbSet.Update(entity); await _context.SaveChangesAsync(); }
    public virtual async Task DeleteAsync(T entity) { _dbSet.Remove(entity); await _context.SaveChangesAsync(); }
    public async Task AddRangeAsync(IEnumerable<T> entities) { await _dbSet.AddRangeAsync(entities); await _context.SaveChangesAsync(); }

    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.AsNoTracking().Where(predicate).ToListAsync();

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.CountAsync(predicate);

    public async Task<List<TResult>> GroupByAsync<TKey, TResult>(
        Expression<Func<T, TKey>> keySelector,
        Expression<Func<IGrouping<TKey, T>, TResult>> resultSelector)
    {
        return await _dbSet.AsNoTracking()
            .GroupBy(keySelector)
            .Select(resultSelector)
            .ToListAsync();
    }

    public async Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var keyProperty = typeof(T).GetProperties()
            .FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)
                              || p.Name.Equals($"{typeof(T).Name}Id", StringComparison.OrdinalIgnoreCase));

        if (keyProperty == null)
            throw new InvalidOperationException($"No primary key property found on type {typeof(T).Name}");

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, keyProperty);
        var containsMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(Guid));

        var idsExpression = Expression.Constant(ids);
        var body = Expression.Call(containsMethod, idsExpression, property);
        var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);

        return await _dbSet.AsNoTracking().Where(lambda).ToListAsync();
    }

    public IQueryable<T> Query()
    {
        return _dbSet.AsNoTracking();
    }
}

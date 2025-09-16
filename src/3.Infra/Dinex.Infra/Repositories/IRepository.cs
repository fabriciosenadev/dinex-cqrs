namespace Dinex.Infra;

public interface IRepository<T> where T: BaseEntity
{
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task UpdateRangeAsync(IEnumerable<T> entities);
    Task SaveChangesAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task<PagedResult<T>> GetPagedAsync(
        Expression<Func<T, bool>>? filter,
        int page,
        int pageSize,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

}

namespace Dinex.Infra;

public interface IRepository<T> : IDisposable where T : Entity
{
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entity);
    Task UpdateRangeAsync(IEnumerable<T> entity);
    Task SaveChangesAsync();
}

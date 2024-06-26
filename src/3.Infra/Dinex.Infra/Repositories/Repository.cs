namespace Dinex.Infra;

public abstract class Repository<T> : IRepository<T> where T : Entity//, new()
{
    protected readonly DbSet<T> _dbSet;
    protected readonly DinexApiContext _context;

    protected Repository(DinexApiContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync();
    }

    //public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    //{
    //    var query = _dbSet.AsNoTracking().AsQueryable();
    //    var filteredData = query.Where(predicate);

    //    // Avaliação no lado do cliente
    //    return await filteredData.ToListAsync();
    //}


    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    //public virtual async Task<List<T>> GetPagedAsync(int pageNumber, int pageSize)
    //{
    //    return await _dbSet.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
    //}

    public virtual async Task AddAsync(T entity)
    {
        _dbSet.Add(entity);
        await SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await SaveChangesAsync();
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await SaveChangesAsync();
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entity)
    {
        _dbSet.AddRange(entity);
        await SaveChangesAsync();
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<T> entity)
    {
        _dbSet.UpdateRange(entity);
        await SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}

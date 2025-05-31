namespace Dinex.Infra;

public class Repository<T> : IRepository<T> where T : class //Entity, new()
{
    private readonly DinexApiContext _context;

    public Repository(DinexApiContext context)
    {
        _context = context;
    }

    public async Task AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _context.Set<T>().AddRangeAsync(entities);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        _context.Set<T>().UpdateRange(entities);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _context.Set<T>().Where(predicate).ToListAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    //protected readonly DbSet<T> _dbSet;
    //protected readonly DinexApiContext _context;

    //public Repository(DinexApiContext context)
    //{
    //    _context = context;
    //    _dbSet = context.Set<T>();
    //}

    //public async virtual Task AddAsync(T entity)
    //{
    //    _dbSet.Add(entity);
    //    await SaveChangesAsync();
    //}

    //public async virtual Task DeleteAsync(T entity)
    //{
    //    _dbSet.Remove(entity);
    //    await SaveChangesAsync();
    //}

    //public async virtual Task UpdateAsync(T entity)
    //{
    //    _dbSet.Update(entity);
    //    await SaveChangesAsync();
    //}

    //public virtual async Task AddRangeAsync(IEnumerable<T> entity)
    //{
    //    _dbSet.AddRange(entity);
    //    await SaveChangesAsync();
    //}

    //public virtual async Task UpdateRangeAsync(IEnumerable<T> entity)
    //{
    //    _dbSet.UpdateRange(entity);
    //    await SaveChangesAsync();
    //}

    //public async Task SaveChangesAsync()
    //{
    //    await _context.SaveChangesAsync();
    //}

    #region not implemented
    //public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    //{
    //    var query = _dbSet.AsNoTracking().AsQueryable();
    //    var filteredData = query.Where(predicate);

    //    // Avaliação no lado do cliente
    //    return await filteredData.ToListAsync();
    //}

    //public virtual async Task<List<T>> GetPagedAsync(int pageNumber, int pageSize)
    //{
    //    return await _dbSet.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
    //}
    #endregion

    #region removed implementation
    //public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    //{
    //    return await _dbSet.AsNoTracking().Where(predicate).ToListAsync();
    //}

    //public virtual async Task<T?> GetByIdAsync(Guid id)
    //{
    //    return await _dbSet.FindAsync(id);
    //}

    //public virtual async Task<List<T>> GetAllAsync()
    //{
    //    return await _dbSet.ToListAsync();
    //}



    //public virtual async Task AddAsync(T entity)
    //{
    //    _dbSet.Add(entity);
    //    await SaveChangesAsync();
    //}

    //public virtual async Task DeleteAsync(T entity)
    //{
    //    _dbSet.Remove(entity);
    //    await SaveChangesAsync();
    //}

    //public virtual async Task UpdateAsync(T entity)
    //{
    //    _dbSet.Update(entity);
    //    await SaveChangesAsync();
    //}

    //public virtual async Task AddRangeAsync(IEnumerable<T> entity)
    //{
    //    _dbSet.AddRange(entity);
    //    await SaveChangesAsync();
    //}

    //public virtual async Task UpdateRangeAsync(IEnumerable<T> entity)
    //{
    //    _dbSet.UpdateRange(entity);
    //    await SaveChangesAsync();
    //}

    //public async Task SaveChangesAsync()
    //{
    //    await _context.SaveChangesAsync();
    //}

    //public void Dispose()
    //{
    //    _context?.Dispose();
    //}
    #endregion
}

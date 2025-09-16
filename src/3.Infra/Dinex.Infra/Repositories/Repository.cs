namespace Dinex.Infra;

public class Repository<T> : IRepository<T> where T : BaseEntity
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

    public async Task<PagedResult<T>> GetPagedAsync(
        Expression<Func<T, bool>>? filter,
        int page,
        int pageSize,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        if (page < 1)
            throw new ArgumentException("Page deve ser maior ou igual a 1.");

        if (pageSize < 1)
            throw new ArgumentException("PageSize deve ser maior ou igual a 1.");

        var query = _context.Set<T>().AsQueryable();

        if (filter != null)
            query = query.Where(filter);

        // Total de itens ANTES da paginação
        var totalCount = await query.CountAsync();

        // Ordenação padrão por CreatedAt se não for fornecida
        if (orderBy != null)
            query = orderBy(query);
        else
            query = query.OrderBy(x => x.CreatedAt);

        // Paginação (lembrando que page começa em 1)
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        var query = _context.Set<T>().AsQueryable();
        if (predicate != null)
            query = query.Where(predicate);

        return await query.CountAsync();
    }
}

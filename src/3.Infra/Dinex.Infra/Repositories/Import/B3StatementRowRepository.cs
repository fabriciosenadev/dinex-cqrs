namespace Dinex.Infra;

public class B3StatementRowRepository : IB3StatementRowRepository
{
    private readonly IRepository<B3StatementRow> _repository;

    public B3StatementRowRepository(IRepository<B3StatementRow> repository)
    {
        _repository = repository;
    }

    public async Task<B3StatementRow?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task AddRangeAsync(IEnumerable<B3StatementRow> rows)
    {
        await _repository.AddRangeAsync(rows);
    }

    public async Task<IEnumerable<B3StatementRow>> GetByImportJobIdAsync(Guid importJobId)
    {
        return await _repository.FindAsync(x => x.ImportJobId == importJobId);
    }

    public async Task<int> CountByImportJobAsync(Guid importJobId)
        => await _repository.CountAsync(x => x.ImportJobId == importJobId);


    public async Task SaveChangesAsync()
    {
        await _repository.SaveChangesAsync();
    }

    public async Task<PagedResult<B3StatementRow>> GetErrorRowsByJobAsync(
            Guid importJobId,
            int page,
            int pageSize,
            string? search,
            string orderBy,
            bool desc,
            bool includeRaw)
    {
        var s = (search ?? string.Empty).Trim();
        var sLower = s.ToLowerInvariant();
        var hasSearch = !string.IsNullOrWhiteSpace(sLower);

        Expression<Func<B3StatementRow, bool>> filter = x =>
            x.ImportJobId == importJobId &&
            x.DeletedAt == null &&
            (x.Error != null || x.Status == B3StatementRowStatus.Erro) &&
            (
                !hasSearch
                ? true
                : includeRaw
                    ? ((x.Error != null && x.Error.ToLower().Contains(sLower)) ||
                       (x.RawLineJson != null && x.RawLineJson.ToLower().Contains(sLower)))
                    : (x.Error != null && x.Error.ToLower().Contains(sLower))
            );

        Func<IQueryable<B3StatementRow>, IOrderedQueryable<B3StatementRow>> order = q =>
        {
            var key = (orderBy ?? "RowNumber").ToLowerInvariant();
            return key switch
            {
                "createdat" => desc ? q.OrderByDescending(x => x.CreatedAt)
                                     : q.OrderBy(x => x.CreatedAt),
                "linenumber" => desc ? q.OrderByDescending(x => x.RowNumber)
                                     : q.OrderBy(x => x.RowNumber), // alias legado
                _ => desc ? q.OrderByDescending(x => x.RowNumber)
                                     : q.OrderBy(x => x.RowNumber),
            };
        };

        // delega paginação pro repositório base → retorna Dinex.Core.PagedResult<B3StatementRow>
        return await _repository.GetPagedAsync(filter, page, pageSize, order);
    }
}

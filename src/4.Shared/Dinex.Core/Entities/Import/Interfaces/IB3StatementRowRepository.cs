namespace Dinex.Core;

public interface IB3StatementRowRepository
{
    Task<B3StatementRow?> GetByIdAsync(Guid id);
    Task AddRangeAsync(IEnumerable<B3StatementRow> rows);
    Task<IEnumerable<B3StatementRow>> GetByImportJobIdAsync(Guid importJobId);
    Task<int> CountByImportJobAsync(Guid importJobId);
    Task SaveChangesAsync();
    Task<PagedResult<B3ErrorFragmentView>> GetErrorFragmentsByJobAsync(
        Guid importJobId,
        int page,
        int pageSize,
        string? search,
        string orderBy,
        bool desc,
        bool includeRaw);
}


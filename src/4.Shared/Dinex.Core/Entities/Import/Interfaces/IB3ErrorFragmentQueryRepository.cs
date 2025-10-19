namespace Dinex.Core;

public interface IB3ErrorFragmentQueryRepository
{
    Task<PagedResult<B3ErrorFragmentReadModel>> GetErrorFragmentsByJobAsync(
        Guid importJobId, int page, int pageSize,
        string? search, string? orderBy, bool desc, bool includeRaw);
}

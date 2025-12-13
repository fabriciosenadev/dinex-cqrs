namespace Dinex.Core;

public interface IAssetRepository
{
    Task AddAsync(Asset asset);
    Task UpdateAsync(Asset asset);
    Task<Asset?> GetByIdAsync(Guid id);
    Task<IEnumerable<Asset>> GetAllAsync();
    Task<Asset?> GetByCodeAsync(string code);
    Task SaveChangesAsync();
    Task<PagedResult<Asset>> GetPagedAsync(
        Expression<Func<Asset, bool>>? filter,
        int page,
        int pageSize,
        Func<IQueryable<Asset>, IOrderedQueryable<Asset>>? orderBy = null);
    Task<IEnumerable<Asset>> GetByIdsAsync(IEnumerable<Guid> ids);
}

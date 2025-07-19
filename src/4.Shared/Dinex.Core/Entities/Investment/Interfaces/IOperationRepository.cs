namespace Dinex.Core;

public interface IOperationRepository
{
    Task AddAsync(Operation operation);
    Task UpdateAsync(Operation operation);
    Task<Operation?> GetByIdAsync(Guid id);
    Task<IEnumerable<Operation>> GetByWalletAsync(Guid walletId);
    Task<IEnumerable<Operation>> GetByAssetAsync(Guid assetId);
    Task<IEnumerable<Operation>> GetByWalletAndAssetAsync(Guid walletId, Guid assetId);
    Task SaveChangesAsync();
    Task<PagedResult<Operation>> GetPagedAsync(
        Expression<Func<Operation, bool>>? filter,
        int page,
        int pageSize,
        Func<IQueryable<Operation>, IOrderedQueryable<Operation>>? orderBy = null);
}

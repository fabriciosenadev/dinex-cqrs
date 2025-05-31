namespace Dinex.Core;

public interface IPositionRepository
{
    Task AddAsync(Position position);
    Task UpdateAsync(Position position);
    Task<Position?> GetByIdAsync(Guid id);
    Task<Position?> GetByWalletAndAssetAsync(Guid walletId, Guid assetId);
    Task<IEnumerable<Position>> GetByWalletAsync(Guid walletId);
    Task SaveChangesAsync();
}



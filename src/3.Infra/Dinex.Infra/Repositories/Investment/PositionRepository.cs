namespace Dinex.Infra;

public class PositionRepository : IPositionRepository
{
    private readonly IRepository<Position> _repository;

    public PositionRepository(IRepository<Position> repository)
    {
        _repository = repository;
    }

    public async Task AddAsync(Position position)
    {
        await _repository.AddAsync(position);
    }

    public async Task UpdateAsync(Position position)
    {
        await _repository.UpdateAsync(position);
    }

    public async Task DeleteAsync(Guid walletId, Guid assetId)
    {
        var result = await GetByWalletAndAssetAsync(walletId, assetId);
        if (result != null)
            await _repository.DeleteAsync(result);
    }

    public async Task<Position?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Position?> GetByWalletAndAssetAsync(Guid walletId, Guid assetId)
    {
        var result = await _repository.FindAsync(p =>
            p.WalletId == walletId &&
            p.AssetId == assetId &&
            p.DeletedAt == null
        );

        return result.FirstOrDefault();
    }

    public async Task<IEnumerable<Position>> GetByWalletAsync(Guid walletId)
    {
        var result = await _repository.FindAsync(p =>
            p.WalletId == walletId &&
            p.DeletedAt == null
        );

        return result;
    }

    public async Task<Position?> GetAnyByWalletAndAssetAsync(Guid walletId, Guid assetId)
    {
        var result = await _repository.FindAsync(p =>
            p.WalletId == walletId &&
            p.AssetId == assetId
        );

        return result.FirstOrDefault();
    }

    public async Task SaveChangesAsync()
    {
        await _repository.SaveChangesAsync();
    }
}


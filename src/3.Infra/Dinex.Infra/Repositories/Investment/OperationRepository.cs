namespace Dinex.Infra;

public class OperationRepository : IOperationRepository
{
    private readonly IRepository<Operation> _repository;

    public OperationRepository(IRepository<Operation> repository)
    {
        _repository = repository;
    }

    public async Task AddAsync(Operation operation)
    {
        await _repository.AddAsync(operation);
    }

    public async Task UpdateAsync(Operation operation)
    {
        await _repository.UpdateAsync(operation);
    }

    public async Task<Operation?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Operation>> GetByWalletAsync(Guid walletId)
    {
        var result = await _repository.FindAsync(o => o.WalletId == walletId);
        return result;
    }

    public async Task<IEnumerable<Operation>> GetByAssetAsync(Guid assetId)
    {
        var result = await _repository.FindAsync(o => o.AssetId == assetId);
        return result;
    }

    public async Task<IEnumerable<Operation>> GetByWalletAndAssetAsync(Guid walletId, Guid assetId)
    {
        var result = await _repository.FindAsync(o => o.WalletId == walletId && o.AssetId == assetId);
        return result;
    }

    public async Task SaveChangesAsync()
    {
        await _repository.SaveChangesAsync();
    }
}


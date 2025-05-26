namespace Dinex.Infra;

public class AssetRepository : IAssetRepository
{
    private readonly IRepository<Asset> _repository;

    public AssetRepository(IRepository<Asset> repository)
    {
        _repository = repository;
    }

    public async Task AddAsync(Asset asset)
    {
        await _repository.AddAsync(asset);
    }

    public async Task UpdateAsync(Asset asset)
    {
        await _repository.UpdateAsync(asset);
    }

    public async Task<Asset?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Asset>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Asset?> GetByCodeAsync(string code)
    {
        var result = await _repository.FindAsync(x => x.Code == code);
        return result.FirstOrDefault();
    }

    public async Task SaveChangesAsync()
    {
        await _repository.SaveChangesAsync();
    }
}


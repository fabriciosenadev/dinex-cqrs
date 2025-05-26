namespace Dinex.Core;

public interface IAssetRepository
{
    Task AddAsync(Asset asset);
    Task UpdateAsync(Asset asset);
    Task<Asset?> GetByIdAsync(Guid id);
    Task<IEnumerable<Asset>> GetAllAsync();
    Task<Asset?> GetByCodeAsync(string code);
    Task SaveChangesAsync();
}

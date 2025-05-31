namespace Dinex.Core;

public interface IWalletRepository
{
    Task AddAsync(Wallet wallet);
    Task UpdateAsync(Wallet wallet);
    Task<Wallet?> GetByIdAsync(Guid id);
    Task<IEnumerable<Wallet>> GetByUserAsync(Guid userId);
    Task SaveChangesAsync();
}

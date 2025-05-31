namespace Dinex.Infra;

public class WalletRepository : IWalletRepository
{
    private readonly IRepository<Wallet> _repository;

    public WalletRepository(IRepository<Wallet> repository)
    {
        _repository = repository;
    }

    public async Task AddAsync(Wallet wallet)
    {
        await _repository.AddAsync(wallet);
    }

    public async Task UpdateAsync(Wallet wallet)
    {
        await _repository.UpdateAsync(wallet);
    }

    public async Task<Wallet?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Wallet>> GetByUserAsync(Guid userId)
    {
        var result = await _repository.FindAsync(w => w.UserId == userId);
        return result;
    }

    public async Task SaveChangesAsync()
    {
        await _repository.SaveChangesAsync();
    }
}


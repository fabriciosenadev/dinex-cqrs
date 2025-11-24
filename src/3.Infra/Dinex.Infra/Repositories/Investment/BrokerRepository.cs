namespace Dinex.Infra;

public class BrokerRepository : IBrokerRepository
{
    private readonly IRepository<Broker> _repository;

    public BrokerRepository(IRepository<Broker> repository)
    {
        _repository = repository;
    }

    public async Task AddAsync(Broker broker)
    {
        await _repository.AddAsync(broker);
    }

    public async Task UpdateAsync(Broker broker)
    {
        await _repository.UpdateAsync(broker);
    }

    public async Task<Broker?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Broker?> GetByCnpjAsync(string cnpj)
    {
        var result = await _repository.FindAsync(x => x.Cnpj == cnpj);
        return result.FirstOrDefault();
    }

    public async Task<IEnumerable<Broker>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _repository.SaveChangesAsync();
    }

    public async Task<Broker?> GetByNormalizedNameAsync(string normalizedBrokername)
    {
        var result = await _repository.FindAsync(x => x.Name == normalizedBrokername);
        return result.FirstOrDefault();
    }
}

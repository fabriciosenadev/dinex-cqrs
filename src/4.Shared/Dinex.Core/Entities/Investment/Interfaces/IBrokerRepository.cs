namespace Dinex.Core;

public interface IBrokerRepository
{
    Task AddAsync(Broker broker);
    Task UpdateAsync(Broker broker);
    Task<Broker?> GetByIdAsync(Guid id);
    Task<Broker?> GetByCnpjAsync(string cnpj);
    Task<IEnumerable<Broker>> GetAllAsync();
    Task SaveChangesAsync();
    Task<Broker?> GetByNormalizedNameAsync(string normalizedBrokername);
}

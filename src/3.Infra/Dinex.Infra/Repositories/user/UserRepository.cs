namespace Dinex.Infra;

public interface IUserRepository
{
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
}

public class UserRepository : IUserRepository
{
    private readonly IRepository<User> _repository;

    public UserRepository(IRepository<User> repository)
    {
        _repository = repository;
    }

    public async Task AddUserAsync(User user)
    {
        await _repository.AddAsync(user);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task UpdateUserAsync(User user)
    {
        await _repository.UpdateAsync(user);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var result = await _repository.FindAsync(w => w.Email == email);
        return result.FirstOrDefault();
    }
}

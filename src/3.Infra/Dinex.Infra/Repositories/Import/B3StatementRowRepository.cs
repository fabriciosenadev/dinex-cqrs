namespace Dinex.Infra;

public class B3StatementRowRepository : IB3StatementRowRepository
{
    private readonly IRepository<B3StatementRow> _repository;

    public B3StatementRowRepository(
        IRepository<B3StatementRow> repository)
    {
        _repository = repository;
    }


    public async Task<B3StatementRow?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task AddRangeAsync(IEnumerable<B3StatementRow> rows)
    {
        await _repository.AddRangeAsync(rows);
    }

    public async Task<IEnumerable<B3StatementRow>> GetByImportJobIdAsync(Guid importJobId)
    {
        return await _repository.FindAsync(x => x.ImportJobId == importJobId);
    }

    public async Task<int> CountByImportJobAsync(Guid importJobId)
        => await _repository.CountAsync(x => x.ImportJobId == importJobId);


    public async Task SaveChangesAsync()
    {
        await _repository.SaveChangesAsync();
    }

    public Task DeleteAsync(B3StatementRow row) => _repository.DeleteAsync(row);
}

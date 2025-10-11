namespace Dinex.Infra;

public class ImportJobRepository : IImportJobRepository
{
    private readonly IRepository<ImportJob> _repository;

    public ImportJobRepository(IRepository<ImportJob> repository)
    {
        _repository = repository;
    }

    public async Task AddAsync(ImportJob job)
    {
        await _repository.AddAsync(job);
    }

    public async Task UpdateAsync(ImportJob job)
    {
        await _repository.UpdateAsync(job);
    }

    public async Task<ImportJob?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<ImportJob>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _repository.SaveChangesAsync();
    }

    // Método específico de contagem
    public async Task<int> CountAsync()
    {
        var jobs = await _repository.GetAllAsync();
        return jobs.Count();
    }

    // Buscar por status
    public async Task<IEnumerable<ImportJob>> GetByStatusAsync(ImportJobStatus status)
    {
        return await _repository.FindAsync(j => j.Status == status);
    }

    // Buscar por usuário
    public async Task<IEnumerable<ImportJob>> GetByUserAsync(string userId)
    {
        return await _repository.FindAsync(j => j.UploadedBy == userId);
    }

    // Buscar os mais recentes (exemplo: últimos 5)
    public async Task<IEnumerable<ImportJob>> GetRecentJobsAsync(int count)
    {
        // Busca todos e ordena em memória — se o volume crescer, considere criar um método próprio no repo genérico para order/limit
        var allJobs = await _repository.GetAllAsync();
        return allJobs.OrderByDescending(j => j.UploadedAt).Take(count);
    }

    public Task DeleteAsync(ImportJob job) => _repository.DeleteAsync(job);
}

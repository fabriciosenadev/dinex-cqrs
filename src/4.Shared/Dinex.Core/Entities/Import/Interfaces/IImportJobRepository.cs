namespace Dinex.Core;

public interface IImportJobRepository
{
    Task<ImportJob?> GetByIdAsync(Guid id);
    Task AddAsync(ImportJob job);
    Task UpdateAsync(ImportJob job);
    Task<IEnumerable<ImportJob>> GetAllAsync();
    Task<int> CountAsync();
    Task SaveChangesAsync();

    // Métodos específicos
    Task<IEnumerable<ImportJob>> GetByStatusAsync(ImportJobStatus status);
    Task<IEnumerable<ImportJob>> GetByUserAsync(string userId);
    Task<IEnumerable<ImportJob>> GetRecentJobsAsync(int count);
}


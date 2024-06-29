namespace Dinex.AppService;

public interface IQueueService
{
    Task<OperationResult> CreateQueueIn(Guid userId, TransactionActivity queueType, IFormFile file);
}

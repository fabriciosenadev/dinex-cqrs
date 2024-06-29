namespace Dinex.AppService;

public class UploadFileCommand : IRequest<OperationResult>
{
    public TransactionActivity QueueType { get; set; }
    public required IFormFile FileHistory { get; set; }
    public Guid UserId { get; set; }
}

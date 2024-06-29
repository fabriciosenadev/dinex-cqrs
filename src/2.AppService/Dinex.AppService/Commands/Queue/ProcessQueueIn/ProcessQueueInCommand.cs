namespace Dinex.AppService;

public class ProcessQueueInCommand : IRequest<OperationResult>
{
    public Guid UserId { get; set; }

    public ProcessQueueInCommand(Guid userId)
    {
        UserId = userId;
    }
}

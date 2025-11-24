namespace Dinex.AppService;

public sealed record ProcessImportJobCommand(
    Guid ImportJobId,
    Guid WalletId,
    BrokerResolutionMode BrokerMode,
    Guid? BrokerId
) : IRequest<OperationResult<ProcessReport>>;

public sealed class ProcessImportJobRequest
{
    public Guid WalletId { get; set; }
    public BrokerResolutionMode BrokerMode { get; set; }
    public Guid? BrokerId { get; set; }
}

public sealed class ProcessReport
{
    public Guid ImportJobId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime FinishedAt { get; set; }

    public int Processed { get; set; }
    public int Errors { get; set; }
    public int Skipped { get; set; }
}

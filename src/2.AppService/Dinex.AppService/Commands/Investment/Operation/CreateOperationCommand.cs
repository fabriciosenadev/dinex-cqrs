namespace AppService;

public record CreateOperationCommand(
    Guid WalletId,
    Guid AssetId,
    Guid? BrokerId,
    OperationType Type,
    decimal Quantity,
    decimal UnitPrice,
    DateTime ExecutedAt
) : IRequest<OperationResult<Guid>>;

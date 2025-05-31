namespace AppService;

public record UpdateOperationCommand(
    Guid Id,
    Guid WalletId,
    Guid AssetId,
    OperationType Type,
    decimal Quantity,
    decimal UnitPrice,
    DateTime ExecutedAt,
    Guid? BrokerId
) : IRequest<OperationResult<Guid>>;

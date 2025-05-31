namespace AppService;

public record UpdatePositionCommand(
    Guid Id,
    Guid WalletId,
    Guid AssetId,
    decimal Quantity,
    decimal AveragePrice,
    decimal InvestedValue,
    Guid? BrokerId
) : IRequest<OperationResult<Guid>>;

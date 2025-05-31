namespace Dinex.AppService;

public record CreatePositionCommand(
    Guid WalletId,
    Guid AssetId,
    decimal Quantity,
    decimal AveragePrice,
    decimal InvestedValue,
    Guid? BrokerId
) : IRequest<OperationResult<Guid>>;

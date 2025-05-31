namespace Dinex.AppService;

public record CreateWalletCommand(
    Guid UserId,
    string Name,
    string DefaultCurrency,
    string? Description
) : IRequest<OperationResult<Guid>>;

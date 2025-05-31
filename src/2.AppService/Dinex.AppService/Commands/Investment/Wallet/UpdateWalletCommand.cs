namespace AppService;

public record UpdateWalletCommand(
    Guid Id,
    Guid UserId,
    string Name,
    string DefaultCurrency,
    string? Description
) : IRequest<OperationResult<Guid>>;

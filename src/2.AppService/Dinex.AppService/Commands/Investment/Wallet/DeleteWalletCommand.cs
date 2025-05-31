namespace Dinex.AppService;

public record DeleteWalletCommand(Guid Id) : IRequest<OperationResult<bool>>;

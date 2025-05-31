namespace Dinex.AppService;

public record DeleteBrokerCommand(Guid Id) : IRequest<OperationResult<bool>>;

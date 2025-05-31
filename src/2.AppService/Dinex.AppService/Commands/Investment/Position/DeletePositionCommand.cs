namespace Dinex.AppService;

public record DeletePositionCommand(Guid Id) : IRequest<OperationResult<bool>>;

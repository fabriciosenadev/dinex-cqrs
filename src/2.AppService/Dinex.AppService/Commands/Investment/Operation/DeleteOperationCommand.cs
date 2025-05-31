namespace Dinex.AppService;

public record DeleteOperationCommand(Guid Id) : IRequest<OperationResult<bool>>;

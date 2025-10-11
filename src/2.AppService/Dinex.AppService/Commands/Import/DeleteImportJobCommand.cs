namespace Dinex.AppService;

public sealed record DeleteImportJobCommand(Guid Id) : IRequest<OperationResult<bool>>;

namespace Dinex.AppService;

public record DeleteAssetCommand(Guid Id) : IRequest<OperationResult<bool>>;

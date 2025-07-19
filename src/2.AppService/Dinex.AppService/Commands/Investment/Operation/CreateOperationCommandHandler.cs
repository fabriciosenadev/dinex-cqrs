using Dinex.AppService;

namespace AppService;

public class CreateOperationCommandHandler : ICommandHandler, IRequestHandler<CreateOperationCommand, OperationResult<Guid>>
{
    private readonly IOperationRepository _operationRepository;
    private readonly IPositionService _positionService;

    public CreateOperationCommandHandler(IOperationRepository operationRepository, IPositionService positionService)
    {
        _operationRepository = operationRepository;
        _positionService = positionService;
    }

    public async Task<OperationResult<Guid>> Handle(CreateOperationCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Guid>();

        var operation = Operation.Create(
            request.WalletId,
            request.AssetId,
            request.Type,
            request.Quantity,
            request.UnitPrice,
            request.ExecutedAt,
            request.BrokerId
        );

        if (!operation.IsValid)
        {
            result.AddNotifications(operation.Notifications);
            return result;
        }

        await _operationRepository.AddAsync(operation);

        _positionService.RecalculatePositionAsync(request.WalletId, request.AssetId);

        result.SetData(operation.Id);
        return result;
    }
}

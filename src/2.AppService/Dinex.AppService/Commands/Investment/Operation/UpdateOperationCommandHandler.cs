using AppService;

namespace Dinex.AppService;

public class UpdateOperationCommandHandler : ICommandHandler, IRequestHandler<UpdateOperationCommand, OperationResult<Guid>>
{
    private readonly IOperationRepository _operationRepository;
    private readonly IPositionService _positionService;

    public UpdateOperationCommandHandler(IOperationRepository operationRepository, IPositionService positionService)
    {
        _operationRepository = operationRepository;
        _positionService = positionService;
    }

    public async Task<OperationResult<Guid>> Handle(UpdateOperationCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Guid>();

        var operations = await _operationRepository.GetByWalletAndAssetAsync(request.WalletId, request.AssetId);
        var operation = operations.FirstOrDefault(o => o.Id == request.Id);

        if (operation is null)
        {
            result.AddError("Operação não encontrada.");
            return result;
        }

        operation.EnsureNotDeleted("Operation");

        if (!operation.IsValid)
        {
            result.AddNotifications(operation.Notifications);
            return result;
        }

        operation.Update(
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

        await _operationRepository.UpdateAsync(operation);

        _positionService.RecalculatePositionAsync(request.WalletId, request.AssetId);

        result.SetData(operation.Id);
        return result;
    }
}

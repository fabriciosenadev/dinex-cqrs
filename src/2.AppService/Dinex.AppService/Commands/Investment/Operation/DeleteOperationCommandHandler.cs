namespace Dinex.AppService;

public class DeleteOperationCommandHandler : ICommandHandler, IRequestHandler<DeleteOperationCommand, OperationResult<bool>>
{
    private readonly IOperationRepository _operationRepository;
    private readonly IPositionService _positionService;

    public DeleteOperationCommandHandler(IOperationRepository operationRepository, IPositionService positionService)
    {
        _operationRepository = operationRepository;
        _positionService = positionService;
    }

    public async Task<OperationResult<bool>> Handle(DeleteOperationCommand request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<bool>();

        var operation = await _operationRepository.GetByIdAsync(request.Id);
        if (operation is null)
        {
            result.AddError("Operação não encontrada.");
            return result;
        }

        operation.MarkAsDeleted();

        await _operationRepository.UpdateAsync(operation);

        _positionService.RecalculatePositionAsync(operation.WalletId, operation.AssetId);

        result.SetData(true);
        return result;
    }
}

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

        // Salva os dados antigos para comparar se vai "mover no histórico"
        var oldExecutedAt = operation.ExecutedAt;

        // Atualiza os dados
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

        // Busca todas operações ativas e ordena por ExecutedAt após o update
        var allOperations = (await _operationRepository.GetByWalletAndAssetAsync(request.WalletId, request.AssetId))
                            .Where(o => !o.IsDeleted())
                            .OrderBy(o => o.ExecutedAt)
                            .ToList();

        // Verifica se a operação atualizada é a última da lista
        bool isLast = allOperations.Last().Id == operation.Id;

        // Se a data mudou para antes do último, não pode incremental!
        bool executedAtMovedBack = request.ExecutedAt < oldExecutedAt;

        // Pode usar incremental só se continua sendo a última E não moveu para trás
        if (isLast && !executedAtMovedBack)
        {
            await _positionService.RecalculatePositionAsync(request.WalletId, request.AssetId, operation);
        }
        else
        {
            await _positionService.RecalculatePositionAsync(request.WalletId, request.AssetId);
        }

        result.SetData(operation.Id);
        return result;
    }
}

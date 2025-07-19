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

        // Busca todas as operações (exceto as já deletadas) depois do "delete"
        var operations = (await _operationRepository.GetByWalletAndAssetAsync(operation.WalletId, operation.AssetId))
                         .Where(o => !o.IsDeleted()) // se tiver soft delete
                         .OrderBy(o => o.ExecutedAt)
                         .ToList();

        // Verifica se a operação deletada era a última da lista
        bool wasLast = operations.Count == 0 || operation.ExecutedAt >= operations.Last().ExecutedAt;

        if (wasLast)
        {
            // Como deletou a última, pode atualizar a posição de forma incremental:
            // - recompute usando as N-1 operações restantes (pode ser otimizado no futuro)
            // Mas normalmente, delete da última requer recomputar a posição toda.
            // Aqui, para simplificação e máxima robustez, mantenha full reprocess.
            await _positionService.RecalculatePositionAsync(operation.WalletId, operation.AssetId);
        }
        else
        {
            // Se deletou no meio, precisa full reprocess
            await _positionService.RecalculatePositionAsync(operation.WalletId, operation.AssetId);
        }

        result.SetData(true);
        return result;
    }

}

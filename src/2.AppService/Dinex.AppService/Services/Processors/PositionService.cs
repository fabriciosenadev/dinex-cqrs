namespace Dinex.AppService;

public interface IPositionService
{
    // Para processar uma posição específica
    Task RecalculatePositionAsync(Guid walletId, Guid assetId);

    // Para processar todas as posições de uma carteira
    //Task RecalculateAllPositionsAsync(Guid walletId);

    // Para processar todas as posições do usuário (opcional, via JOIN em carteiras)
    //Task RecalculateAllPositionsAsyncByUser(Guid userId);

}

public class PositionService : IPositionService
{
    private readonly IPositionRepository _positionRepository;
    private readonly IOperationRepository _operationRepository;

    public PositionService(IPositionRepository positionRepository, IOperationRepository operationRepository)
    {
        _positionRepository = positionRepository;
        _operationRepository = operationRepository;
    }

    public Task RecalculateAllPositionsAsync(Guid walletId)
    {
        throw new NotImplementedException();
    }

    public Task RecalculateAllPositionsAsyncByUser(Guid userId)
    {
        throw new NotImplementedException();
    }

    public async Task RecalculatePositionAsync(Guid walletId, Guid assetId)
    {
        var operations = await _operationRepository.GetByWalletAndAssetAsync(walletId, assetId);
        if (operations is null || !operations.Any())
        {
            // Zera/remover a posição
            await _positionRepository.DeleteAsync(walletId, assetId);
            return;
        }

        // Ordena por ExecutedAt
        var sortedOperations = operations.OrderBy(o => o.ExecutedAt);

        // Processa a posição
        decimal quantity = 0, invested = 0, avgPrice = 0;
        foreach (var op in sortedOperations)
        {
            switch (op.Type)
            {
                case OperationType.Buy:
                    invested += op.Quantity * op.UnitPrice;
                    quantity += op.Quantity;
                    avgPrice = quantity > 0 ? invested / quantity : 0;
                    break;
                case OperationType.Sell:
                    invested -= avgPrice * op.Quantity;
                    quantity -= op.Quantity;
                    if (quantity < 0) quantity = 0;
                    if (invested < 0) invested = 0;
                    break;
            }
        }

        // Atualiza ou cria a posição
        var existingPosition = await _positionRepository.GetByWalletAndAssetAsync(walletId, assetId);
        if (existingPosition is null)
        {
            var newPosition = Position.Create(walletId, assetId, quantity, avgPrice, invested);
            await _positionRepository.AddAsync(newPosition);
        }
        else
        {
            existingPosition.Update(quantity, avgPrice, invested);
            await _positionRepository.UpdateAsync(existingPosition);
        }
    }

}

namespace Dinex.AppService;

public interface IPositionService
{
    /// <summary>
    /// Processa/recalcula a posição específica daquele ativo/carteira.
    /// Se a operação for no final do histórico, faz cálculo incremental.
    /// Se for retroativa, reprocessa tudo.
    /// </summary>
    Task RecalculatePositionAsync(Guid walletId, Guid assetId, Operation? triggeringOperation = null);
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

    public async Task RecalculatePositionAsync(Guid walletId, Guid assetId, Operation? triggeringOperation = null)
    {
        var operations = (await _operationRepository.GetByWalletAndAssetAsync(walletId, assetId))
            ?.OrderBy(o => o.ExecutedAt)
            .ToList();

        if (operations is null || !operations.Any())
        {
            await _positionRepository.DeleteAsync(walletId, assetId);
            return;
        }

        // Descobrir se pode ser incremental (operação no final da lista)
        bool podeIncremental = false;
        if (triggeringOperation != null)
        {
            var ultimaData = operations.Last().ExecutedAt;
            // Se a triggeringOperation é a última cronologicamente E não houve update/delete de antigas
            podeIncremental = triggeringOperation.ExecutedAt >= ultimaData && triggeringOperation.Id == operations.Last().Id;
        }

        var position = await _positionRepository.GetByWalletAndAssetAsync(walletId, assetId);

        decimal quantity = 0, invested = 0, avgPrice = 0;

        if (podeIncremental && position != null)
        {
            // Cálculo incremental
            quantity = position.CurrentQuantity;
            avgPrice = position.AveragePrice;
            invested = position.InvestedValue;

            var op = triggeringOperation!;
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
        else
        {
            // Reprocessamento total
            foreach (var op in operations)
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
        }

        if (position is null)
        {
            var newPosition = Position.Create(walletId, assetId, quantity, avgPrice, invested);
            await _positionRepository.AddAsync(newPosition);
        }
        else
        {
            position.Update(quantity, avgPrice, invested);
            await _positionRepository.UpdateAsync(position);
        }
    }
}

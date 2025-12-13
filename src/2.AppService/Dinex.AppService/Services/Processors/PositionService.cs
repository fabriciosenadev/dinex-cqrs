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

    public async Task RecalculatePositionAsync(
        Guid walletId,
        Guid assetId,
        Operation? triggeringOperation = null)
    {
        var operations = (await _operationRepository
                .GetByWalletAndAssetAsync(walletId, assetId))
            ?.OrderBy(o => o.ExecutedAt)
            .ToList();

        // Nenhuma operação → posição encerrada
        if (operations is null || !operations.Any())
        {
            var existing = await _positionRepository
                .GetAnyByWalletAndAssetAsync(walletId, assetId);

            if (existing != null && existing.DeletedAt == null)
            {
                existing.MarkAsDeleted();
                await _positionRepository.UpdateAsync(existing);
            }

            return;
        }

        // Pode ser incremental?
        bool podeIncremental = false;
        if (triggeringOperation != null)
        {
            var ultima = operations.Last();
            podeIncremental =
                triggeringOperation.Id == ultima.Id &&
                triggeringOperation.ExecutedAt >= ultima.ExecutedAt;
        }

        var positionAny =
            await _positionRepository.GetAnyByWalletAndAssetAsync(walletId, assetId);

        decimal quantity = 0;
        decimal invested = 0;
        decimal avgPrice = 0;

        // 🔒 Incremental somente se a posição existir E estiver ativa
        if (podeIncremental && positionAny != null && positionAny.DeletedAt == null)
        {
            quantity = positionAny.CurrentQuantity;
            invested = positionAny.InvestedValue;
            avgPrice = positionAny.AveragePrice;

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

        // 🔚 Posição encerrada → soft delete
        if (quantity == 0)
        {
            if (positionAny != null && positionAny.DeletedAt == null)
            {
                positionAny.MarkAsDeleted();
                await _positionRepository.UpdateAsync(positionAny);
            }
            return;
        }

        // 🔄 Restaura se estava deletada
        if (positionAny != null && positionAny.DeletedAt != null)
        {
            positionAny.Restore();
        }

        // ➕ Cria ou atualiza
        if (positionAny is null)
        {
            var newPosition = Position.Create(
                walletId,
                assetId,
                quantity,
                avgPrice,
                invested
            );

            await _positionRepository.AddAsync(newPosition);
        }
        else
        {
            positionAny.Update(quantity, avgPrice, invested);
            await _positionRepository.UpdateAsync(positionAny);
        }
    }
}

namespace Dinex.AppService;

public class GetAllOperationsByWalletQueryHandler : IQueryHandler, IRequestHandler<GetAllOperationsByWalletQuery, OperationResult<IEnumerable<OperationDTO>>>
{
    private readonly IOperationRepository _operationRepository;

    public GetAllOperationsByWalletQueryHandler(IOperationRepository operationRepository)
    {
        _operationRepository = operationRepository;
    }

    public async Task<OperationResult<IEnumerable<OperationDTO>>> Handle(GetAllOperationsByWalletQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<IEnumerable<OperationDTO>>();

        var operations = await _operationRepository.GetByWalletAsync(request.WalletId);

        var filtered = operations
            .Where(x => x.DeletedAt == null)
            .Select(operation => new OperationDTO
            {
                Id = operation.Id,
                WalletId = operation.WalletId,
                AssetId = operation.AssetId,
                BrokerId = operation.BrokerId,
                Type = operation.Type.ToString(),
                Quantity = operation.Quantity,
                UnitPrice = operation.UnitPrice,
                TotalValue = operation.TotalValue,
                ExecutedAt = operation.ExecutedAt
            });

        result.SetData(filtered);
        return result;
    }
}

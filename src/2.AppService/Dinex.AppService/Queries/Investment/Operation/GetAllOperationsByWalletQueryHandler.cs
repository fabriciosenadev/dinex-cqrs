namespace Dinex.AppService;

public class GetAllOperationsByWalletQueryHandler : IQueryHandler, IRequestHandler<GetAllOperationsByWalletQuery, OperationResult<PagedResult<OperationDTO>>>
{
    private readonly IOperationRepository _operationRepository;

    public GetAllOperationsByWalletQueryHandler(IOperationRepository operationRepository)
    {
        _operationRepository = operationRepository;
    }

    public async Task<OperationResult<PagedResult<OperationDTO>>> Handle(
        GetAllOperationsByWalletQuery request,
        CancellationToken cancellationToken)
    {
        var result = new OperationResult<PagedResult<OperationDTO>>();

        // Filtro: operações dessa carteira e não deletadas
        Expression<Func<Operation, bool>> filter = op =>
            op.WalletId == request.WalletId && op.DeletedAt == null;

        var pagedOps = await _operationRepository.GetPagedAsync(
            filter,
            request.Page,
            request.PageSize,
            q => q.OrderByDescending(o => o.ExecutedAt)
        );

        // Projete para DTO
        var pagedResult = new PagedResult<OperationDTO>
        {
            Items = pagedOps.Items.Select(o => new OperationDTO
            {
                Id = o.Id,
                WalletId = o.WalletId,
                AssetId = o.AssetId,
                BrokerId = o.BrokerId,
                Type = o.Type.ToString(),
                Quantity = o.Quantity,
                UnitPrice = o.UnitPrice,
                TotalValue = o.TotalValue,
                ExecutedAt = o.ExecutedAt
            }).ToList(),
            TotalCount = pagedOps.TotalCount,
            Page = pagedOps.Page,
            PageSize = pagedOps.PageSize
        };

        result.SetData(pagedResult);
        return result;
    }

}

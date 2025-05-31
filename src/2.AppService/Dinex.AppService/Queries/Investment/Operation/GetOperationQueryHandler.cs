namespace Dinex.AppService;

public class GetOperationQueryHandler : IQueryHandler, IRequestHandler<GetOperationQuery, OperationResult<OperationDTO>>
{
    private readonly IOperationRepository _operationRepository;

    public GetOperationQueryHandler(IOperationRepository operationRepository)
    {
        _operationRepository = operationRepository;
    }

    public async Task<OperationResult<OperationDTO>> Handle(GetOperationQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<OperationDTO>();

        var operation = await _operationRepository.GetByIdAsync(request.OperationId);
        if (operation == null)
            return result.AddError("Operation not found").SetAsNotFound();

        operation.EnsureNotDeleted("Operation");

        result.SetData(new OperationDTO
        {
            Id = operation.Id,
            WalletId = operation.WalletId,
            BrokerId = operation.BrokerId,
            AssetId = operation.AssetId,
            Type = operation.Type.ToString(),
            Quantity = operation.Quantity,
            UnitPrice = operation.UnitPrice,
            TotalValue = operation.TotalValue,
            ExecutedAt = operation.ExecutedAt
        });

        return result;
    }
}

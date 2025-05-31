namespace Dinex.AppService;

public class GetPositionQueryHandler : IQueryHandler, IRequestHandler<GetPositionQuery, OperationResult<PositionDTO>>
{
    private readonly IPositionRepository _positionRepository;

    public GetPositionQueryHandler(IPositionRepository positionRepository)
    {
        _positionRepository = positionRepository;
    }

    public async Task<OperationResult<PositionDTO>> Handle(GetPositionQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<PositionDTO>();

        var position = await _positionRepository.GetByIdAsync(request.PositionId);
        if (position == null)
            return result.AddError("Position not found").SetAsNotFound();

        position.EnsureNotDeleted("Position");

        result.SetData(new PositionDTO
        {
            Id = position.Id,
            WalletId = position.WalletId,
            AssetId = position.AssetId,
            BrokerId = position.BrokerId,
            CurrentQuantity = position.CurrentQuantity,
            AveragePrice = position.AveragePrice,
            InvestedValue = position.InvestedValue
        });

        return result;
    }
}

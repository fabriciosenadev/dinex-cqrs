namespace Dinex.AppService;

public class GetAllPositionsByWalletQueryHandler : IQueryHandler, IRequestHandler<GetAllPositionsByWalletQuery, OperationResult<IEnumerable<PositionDTO>>>
{
    private readonly IPositionRepository _positionRepository;

    public GetAllPositionsByWalletQueryHandler(IPositionRepository positionRepository)
    {
        _positionRepository = positionRepository;
    }

    public async Task<OperationResult<IEnumerable<PositionDTO>>> Handle(GetAllPositionsByWalletQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<IEnumerable<PositionDTO>>();

        var positions = await _positionRepository.GetByWalletAsync(request.WalletId);

        var filtered = positions
            .Where(x => x.DeletedAt == null)
            .Select(position => new PositionDTO
            {
                Id = position.Id,
                WalletId = position.WalletId,
                AssetId = position.AssetId,
                BrokerId = position.BrokerId,
                CurrentQuantity = position.CurrentQuantity,
                AveragePrice = position.AveragePrice,
                InvestedValue = position.InvestedValue
            });

        result.SetData(filtered);
        return result;
    }
}

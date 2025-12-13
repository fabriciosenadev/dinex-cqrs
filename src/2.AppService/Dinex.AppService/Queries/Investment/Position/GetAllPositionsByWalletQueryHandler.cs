namespace Dinex.AppService;

public class GetAllPositionsByWalletQueryHandler : IQueryHandler, IRequestHandler<GetAllPositionsByWalletQuery, OperationResult<IEnumerable<PositionDTO>>>
{
    private readonly IPositionRepository _positionRepository;
    private readonly IAssetRepository _assetRepository;

    public GetAllPositionsByWalletQueryHandler(IPositionRepository positionRepository, IAssetRepository assetRepository)
    {
        _positionRepository = positionRepository;
        _assetRepository = assetRepository;
    }

    public async Task<OperationResult<IEnumerable<PositionDTO>>> Handle(
        GetAllPositionsByWalletQuery request,
        CancellationToken cancellationToken)
    {
        var result = new OperationResult<IEnumerable<PositionDTO>>();

        var positions = await _positionRepository.GetByWalletAsync(request.WalletId);

        var filtered = positions
            .Where(x => x.DeletedAt == null && x.CurrentQuantity > 0)
            .ToList();

        if (!filtered.Any())
        {
            result.SetData(Enumerable.Empty<PositionDTO>());
            return result;
        }

        var assetIds = filtered
            .Select(p => p.AssetId)
            .Distinct()
            .ToList();

        // You may already have this. If not, implement it in IAssetRepository.
        var assets = await _assetRepository.GetByIdsAsync(assetIds);

        var assetsById = assets.ToDictionary(a => a.Id, a => a);

        var totalInvested = filtered.Sum(p => p.InvestedValue);

        var dtos = filtered.Select(position =>
        {
            assetsById.TryGetValue(position.AssetId, out var asset);

            var walletSharePercent = totalInvested > 0
                ? Math.Round((position.InvestedValue / totalInvested) * 100m, 2)
                : 0m;

            return new PositionDTO
            {
                Id = position.Id,
                WalletId = position.WalletId,
                AssetId = position.AssetId,
                BrokerId = position.BrokerId,
                CurrentQuantity = position.CurrentQuantity,
                AveragePrice = position.AveragePrice,
                InvestedValue = position.InvestedValue,
                AssetName = asset?.Name,
                AssetCode = asset?.Code,
                AssetType = asset?.Type,
                WalletSharePercent = walletSharePercent
            };
        });

        result.SetData(dtos);
        return result;
    }
}

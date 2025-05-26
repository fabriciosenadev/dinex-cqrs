namespace Dinex.AppService;

public class GetAllAssetsQueryHandler : IQueryHandler, IRequestHandler<GetAllAssetsQuery, OperationResult<IEnumerable<AssetDTO>>>
{
    private readonly IAssetRepository _assetRepository;

    public GetAllAssetsQueryHandler(IAssetRepository assetRepository)
    {
        _assetRepository = assetRepository;
    }

    public async Task<OperationResult<IEnumerable<AssetDTO>>> Handle(GetAllAssetsQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<IEnumerable<AssetDTO>>();

        var assets = await _assetRepository.GetAllAsync();

        var activeAssets = assets
            .Where(asset => asset.DeletedAt == null)
            .Select(asset => new AssetDTO
            {
                Id = asset.Id,
                Name = asset.Name,
                Code = asset.Code,
                Cnpj = asset.Cnpj,
                Exchange = asset.Exchange.ToString(),
                Currency = asset.Currency.ToString(),
                Type = asset.Type.ToString(),
                Sector = asset.Sector
            });

        result.SetData(activeAssets);
        return result;
    }
}

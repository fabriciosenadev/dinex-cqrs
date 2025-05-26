namespace Dinex.AppService;

public class GetAssetQueryHandler : IQueryHandler, IRequestHandler<GetAssetQuery, OperationResult<AssetDTO>>
{
    private readonly IAssetRepository _assetRepository;

    public GetAssetQueryHandler(IAssetRepository assetRepository)
    {
        _assetRepository = assetRepository;
    }

    public async Task<OperationResult<AssetDTO>> Handle(GetAssetQuery request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<AssetDTO>();

        var asset = await _assetRepository.GetByIdAsync(request.AssetId);
        if (asset == null)
            return result.AddError("Asset not found").SetAsNotFound();

        asset.EnsureNotDeleted("Asset");

        result.SetData(new AssetDTO
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

        return result;
    }
}

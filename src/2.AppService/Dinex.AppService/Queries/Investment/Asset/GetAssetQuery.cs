namespace Dinex.AppService;

public class GetAssetQuery : IRequest<OperationResult<AssetDTO>>
{
    public Guid AssetId { get; set; }

    public GetAssetQuery(Guid assetId)
    {
        AssetId = assetId;
    }
}

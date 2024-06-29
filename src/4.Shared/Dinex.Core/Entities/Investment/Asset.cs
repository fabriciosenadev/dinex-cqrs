namespace Dinex.Core;

public class Asset : Entity
{
    public AssetType? AssetType { get; private set; }
    public string Ticker { get; private set; }
    public string CompanyName { get; private set; }
    public string? TaxId { get; private set; }

    private Asset(AssetType? assetType,
        string ticker,
        string companyName,
        string? taxId,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt)
    {
        AssetType = assetType;
        Ticker = ticker;
        CompanyName = companyName;
        TaxId = taxId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DeletedAt = deletedAt;
    }

    public static Asset Create(string ticker, string companyName)
    {
        var newAsset = new Asset(
            assetType: null,
            ticker,
            companyName,
            taxId: null,
            createdAt: DateTime.UtcNow,
            updatedAt: null,
            deletedAt: null);

        return newAsset;
    }

    public static IEnumerable<Asset> CreateRange(IEnumerable<string> rawAssetNames)
    {
        var assetList = new List<Asset>();
        foreach (var rawAssetName in rawAssetNames)
        {
            var splitedName = rawAssetName.Split("-");
            var asset = Asset.Create(
                ticker: splitedName[0].Trim(),
                companyName: splitedName[1].Trim());

            if (!assetList.Any(x => x.Ticker == asset.Ticker))
                assetList.Add(asset);
        }

        return assetList;
    }
}

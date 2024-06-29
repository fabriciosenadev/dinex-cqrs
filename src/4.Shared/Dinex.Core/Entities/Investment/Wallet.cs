namespace Dinex.Core;

public class Wallet : Entity
{
    public Guid UserId { get; private set; }
    public Guid AssetId { get; private set; }
    public int AssetQuantity { get; private set; }
    public decimal InvestedAmount { get; private set; }
    public decimal AveragePrice { get; private set; }

    private Wallet(
        Guid userId,
        Guid assetId,
        int assetQuantity,
        decimal investedAmount,
        decimal averagePrice,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt) 
    { 
        UserId = userId;
        AssetId = assetId;
        AssetQuantity = assetQuantity;
        InvestedAmount = investedAmount;
        AveragePrice = averagePrice;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DeletedAt = deletedAt;
    }

    public static Wallet CreateAsset(Guid userId, Guid assetId, int assetQuantity, decimal investedAmount)
    {
        var averagePrice = investedAmount / assetQuantity;

        var wallet = new Wallet (
            userId,
            assetId, 
            assetQuantity,
            investedAmount,
            averagePrice,
            DateTime.UtcNow,
            null,
            null);

        return wallet;
    }

    public void UpdateAsset(int assetQuantity, decimal investedAmount)
    {
        AssetQuantity += assetQuantity;
        InvestedAmount += investedAmount;
        AveragePrice = InvestedAmount / AssetQuantity;
    }
}

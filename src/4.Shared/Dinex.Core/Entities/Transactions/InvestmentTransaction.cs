namespace Dinex.Core;

public class InvestmentTransaction : Entity
{
    public Guid TransactionHistoryId { get; private set; }
    public Applicable Applicable { get; private set; }
    public InvestmentTransactionType TransactionType { get; private set; }
    public Guid AssetId { get; private set; }
    public decimal AssetUnitPrice { get; private set; }
    public decimal AssetTransactionAmount { get; private set; }
    public int AssetQuantity { get; private set; }
    public Guid StockBrokerId { get; private set; }

    private InvestmentTransaction (
        Guid transactionHistoryId,
        Applicable applicable,
        InvestmentTransactionType transactionType,
        Guid assetId,
        decimal assetUnitPrice,
        decimal assetTransactionAmount,
        int assetQuantity,
        Guid stockBrokerId,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt)
    {
        TransactionHistoryId = transactionHistoryId;
        Applicable = applicable;
        TransactionType = transactionType;
        AssetId = assetId;
        AssetUnitPrice = assetUnitPrice;
        AssetTransactionAmount = assetTransactionAmount;
        AssetQuantity = assetQuantity;
        StockBrokerId = stockBrokerId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DeletedAt = deletedAt;
    }

    public static InvestmentTransaction Create(
    Guid transactionHistoryId,
    Applicable applicable,
    InvestmentTransactionType transactionType,
    Guid assetId,
    decimal unitPrice,
    decimal transactionAmount,
    int assetQuantity,
    Guid stockBrokerId)
    {
        var investment = new InvestmentTransaction(
            transactionHistoryId,
            applicable,
            transactionType,
            assetId,
            unitPrice,
            transactionAmount,
            assetQuantity,
            stockBrokerId,
            DateTime.UtcNow,
            null,
            null);

        return investment;
    }
}

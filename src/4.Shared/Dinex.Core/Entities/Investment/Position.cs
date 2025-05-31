namespace Dinex.Core;

public class Position : Entity
{
    public Guid WalletId { get; private set; }
    public Guid? BrokerId { get; private set; }
    public Guid AssetId { get; private set; }

    public decimal CurrentQuantity { get; private set; }
    public decimal AveragePrice { get; private set; }
    public decimal InvestedValue { get; private set; }

    protected Position() { }

    public static Position Create(
        Guid walletId,
        Guid assetId,
        decimal currentQuantity,
        decimal averagePrice,
        decimal investedValue,
        Guid? brokerId = null)
    {
        var position = new Position
        {
            WalletId = walletId,
            AssetId = assetId,
            BrokerId = brokerId,
            CurrentQuantity = currentQuantity,
            AveragePrice = averagePrice,
            InvestedValue = investedValue,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var contract = new Contract<Position>()
            .Requires()
            .IsNotEmpty(walletId, nameof(WalletId), "Carteira é obrigatória.")
            .IsNotEmpty(assetId, nameof(AssetId), "Ativo é obrigatório.")
            .IsGreaterOrEqualsThan(currentQuantity, 0, nameof(CurrentQuantity), "Quantidade atual não pode ser negativa.")
            .IsGreaterOrEqualsThan(averagePrice, 0, nameof(AveragePrice), "Preço médio não pode ser negativo.")
            .IsGreaterOrEqualsThan(investedValue, 0, nameof(InvestedValue), "Valor investido não pode ser negativo.");

        position.AddNotifications(contract);
        return position;
    }

    public void Update(
        decimal quantity,
        decimal averagePrice,
        decimal investedValue,
        Guid? brokerId = null)
    {
        CurrentQuantity = quantity;
        AveragePrice = averagePrice;
        InvestedValue = investedValue;
        BrokerId = brokerId;
        UpdatedAt = DateTime.UtcNow;

        var contract = new Contract<Position>()
            .Requires()
            .IsGreaterOrEqualsThan(quantity, 0, nameof(CurrentQuantity), "Quantidade atual não pode ser negativa.")
            .IsGreaterOrEqualsThan(averagePrice, 0, nameof(AveragePrice), "Preço médio não pode ser negativo.")
            .IsGreaterOrEqualsThan(investedValue, 0, nameof(InvestedValue), "Valor investido não pode ser negativo.");

        AddNotifications(contract);
    }
}

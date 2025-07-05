namespace Dinex.Core;

public class Operation : Entity
{
    public Guid WalletId { get; private set; }
    public Guid? BrokerId { get; private set; }
    public Guid AssetId { get; private set; }

    public OperationType Type { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalValue { get; private set; }

    public DateTime ExecutedAt { get; private set; }

    protected Operation() { }

    public static Operation Create(
        Guid walletId,
        Guid assetId,
        OperationType type,
        decimal quantity,
        decimal unitPrice,
        DateTime executedAt,
        Guid? brokerId = null)
    {
        var operation = new Operation
        {
            WalletId = walletId,
            BrokerId = brokerId,
            AssetId = assetId,
            Type = type,
            Quantity = quantity,
            UnitPrice = unitPrice,
            ExecutedAt = DateTime.SpecifyKind(executedAt, DateTimeKind.Utc),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            TotalValue = Math.Round(quantity * unitPrice, 2)
        };

        var contract = new Contract<Operation>()
            .Requires()
            .IsNotEmpty(walletId, nameof(WalletId), "Carteira é obrigatória.")
            .IsNotEmpty(assetId, nameof(AssetId), "Ativo é obrigatório.")
            .IsGreaterThan(quantity, 0, nameof(Quantity), "A quantidade deve ser maior que zero.")
            .IsGreaterThan(unitPrice, 0, nameof(UnitPrice), "O preço unitário deve ser maior que zero.")
            .IsLowerOrEqualsThan(operation.TotalValue, 1_000_000_000, nameof(TotalValue), "Valor total ultrapassa o limite permitido.");

        operation.AddNotifications(contract);
        return operation;
    }

    public void Update(
        OperationType type,
        decimal quantity,
        decimal unitPrice,
        DateTime executedAt,
        Guid? brokerId = null)
    {
        Type = type;
        Quantity = quantity;
        UnitPrice = unitPrice;
        ExecutedAt = DateTime.SpecifyKind(executedAt, DateTimeKind.Utc);
        BrokerId = brokerId;
        TotalValue = Math.Round(quantity * unitPrice, 2);
        UpdatedAt = DateTime.UtcNow;

        var contract = new Contract<Operation>()
            .Requires()
            .IsGreaterThan(quantity, 0, nameof(Quantity), "A quantidade deve ser maior que zero.")
            .IsGreaterThan(unitPrice, 0, nameof(UnitPrice), "O preço unitário deve ser maior que zero.")
            .IsLowerOrEqualsThan(TotalValue, 1_000_000_000, nameof(TotalValue), "Valor total ultrapassa o limite permitido.");

        AddNotifications(contract);
    }

}

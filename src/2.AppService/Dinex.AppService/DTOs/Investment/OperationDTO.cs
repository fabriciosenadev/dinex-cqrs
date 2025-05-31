namespace Dinex.AppService;

public class OperationDTO
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Guid AssetId { get; set; }
    public Guid? BrokerId { get; set; }
    public string Type { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalValue { get; set; }
    public DateTime ExecutedAt { get; set; }
}

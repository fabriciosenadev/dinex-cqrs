namespace Dinex.AppService;

public class PositionDTO
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Guid AssetId { get; set; }
    public Guid? BrokerId { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal InvestedValue { get; set; }
}

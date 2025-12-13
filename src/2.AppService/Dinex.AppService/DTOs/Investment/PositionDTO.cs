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

    // Extra fields for UI
    public string? AssetName { get; set; }
    public string? AssetCode { get; set; }
    public AssetType? AssetType { get; set; }

    /// <summary>
    /// Percentage of the wallet invested in this position (0–100).
    /// </summary>
    public decimal WalletSharePercent { get; set; }
}

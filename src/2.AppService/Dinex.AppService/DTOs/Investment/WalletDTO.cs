namespace Dinex.AppService;

public class WalletDTO
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string DefaultCurrency { get; set; } = null!;
}

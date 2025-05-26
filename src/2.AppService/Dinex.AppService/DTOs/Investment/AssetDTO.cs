namespace Dinex.AppService;

public class AssetDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Cnpj { get; set; }
    public string Exchange { get; set; } = null!;
    public string Currency { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string? Sector { get; set; }
}

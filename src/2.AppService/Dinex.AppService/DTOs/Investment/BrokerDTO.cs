namespace Dinex.AppService;

public class BrokerDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Cnpj { get; set; }
}

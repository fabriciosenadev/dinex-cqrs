namespace Dinex.AppService;

public sealed class ImportErrorDTO
{
    public Guid Id { get; init; }
    public Guid ImportJobId { get; init; }
    public int? LineNumber { get; init; }
    public string Error { get; init; } = string.Empty;
    public string? RawLineJson { get; init; }           // só vem quando includeRaw = true
    public DateTime CreatedAt { get; init; }
}

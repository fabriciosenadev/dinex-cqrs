namespace Dinex.AppService;

public sealed class ImportRowForEditDTO
{
    public Guid Id { get; init; }                 // RowId (igual ao da entidade)
    public Guid ImportJobId { get; init; }
    public int RowNumber { get; init; }

    public string? Asset { get; init; }
    public string? OperationType { get; init; }   // enum -> string p/ front
    public string? Movement { get; init; }

    public DateTime Date { get; init; }
    public string? DueDate { get; init; }
    public decimal? Quantity { get; init; }
    public decimal? UnitPrice { get; init; }
    public decimal? TotalValue { get; init; }

    public string? Broker { get; init; }
    public string RawLineJson { get; init; } = null!;
    public string Status { get; init; } = string.Empty;
    public string? Error { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

namespace Dinex.AppService;

public class ImportJobListItemDTO
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = default!;
    public DateTime UploadedAt { get; set; }
    public string Status { get; set; } = default!;

    public int? TotalRows { get; set; }
    public int ImportedRows { get; set; }
    public int ErrorsCount { get; set; }
    public DateTime PeriodStartUtc { get; set; }
    public DateTime PeriodEndUtc { get; set; }

    public int TotalTradeRows { get; set; }
    public int ProcessedTradeRows { get; set; }
    public int RemainingTradeRows { get; set; }
}

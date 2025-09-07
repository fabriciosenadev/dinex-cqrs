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
}

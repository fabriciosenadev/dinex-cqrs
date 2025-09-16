namespace Dinex.Core;

public sealed class B3ErrorFragmentView : BaseEntity
{
    public Guid RowId { get; set; }
    public Guid ImportJobId { get; set; }
    public int? RowNumber { get; set; }
    public string Error { get; set; } = null!;
    public string? RawLineJson { get; set; }
    public int ErrorIndex { get; set; }
}

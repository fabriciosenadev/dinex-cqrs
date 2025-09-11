namespace Dinex.AppService;

public sealed class GetImportErrorsQuery : IRequest<OperationResult<PagedResult<ImportErrorDTO>>>, IQueryHandler
{
    public Guid ImportJobId { get; set; }
    public int Page { get; set; }              // sem default aqui
    public int PageSize { get; set; }          // sem default aqui
    public string? Search { get; set; }
    public string? OrderBy { get; set; }       // sem default aqui
    public bool Desc { get; set; }             // sem default aqui
    public bool IncludeRaw { get; set; }       // sem default aqui
}


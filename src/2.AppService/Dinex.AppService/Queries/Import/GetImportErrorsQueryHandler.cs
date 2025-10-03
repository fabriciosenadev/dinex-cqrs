namespace Dinex.AppService;

public sealed class GetImportErrorsQueryHandler
    : IRequestHandler<GetImportErrorsQuery, OperationResult<PagedResult<ImportErrorDTO>>>,
      IQueryHandler
{
    private readonly IB3StatementRowRepository _repo;

    public GetImportErrorsQueryHandler(IB3StatementRowRepository repo) => _repo = repo;

    public async Task<OperationResult<PagedResult<ImportErrorDTO>>> Handle(
        GetImportErrorsQuery query, CancellationToken ct)
    {
        var result = new OperationResult<PagedResult<ImportErrorDTO>>();
        if (!NormalizeAndValidate(query, result)) return result;

        var page = await _repo.GetErrorFragmentsByJobAsync(
            query.ImportJobId, query.Page, query.PageSize,
            query.Search, query.OrderBy!, query.Desc, query.IncludeRaw);

        var items = page.Items.Select(x => new ImportErrorDTO
        {
            Id = x.RowId,
            ImportJobId = x.ImportJobId,
            LineNumber = x.RowNumber,
            Error = x.Error,
            RawLineJson = query.IncludeRaw ? x.RawLineJson : null,
            CreatedAt = x.CreatedAt
        });

        return result.SetData(new PagedResult<ImportErrorDTO>
        {
            Items = items,
            TotalCount = page.TotalCount,
            Page = page.Page,
            PageSize = page.PageSize
        });
    }

    /// <summary>
    /// Normaliza paginação/ordenação e adiciona erros no MESMO OperationResult.
    /// Retorna true se estiver válido após normalização.
    /// </summary>
    private static bool NormalizeAndValidate(
        GetImportErrorsQuery query,
        OperationResult<PagedResult<ImportErrorDTO>> result)
    {
        // Normalização (defaults)
        if (query.Page < 1) query.Page = 1;
        if (query.PageSize < 1 || query.PageSize > 200) query.PageSize = 50;
        if (string.IsNullOrWhiteSpace(query.OrderBy)) query.OrderBy = "RowNumber";

        // Validação mínima
        if (query.Page < 1 || query.PageSize is < 1 or > 200)
        {
            result.AddError("Invalid pagination. Page must be >= 1 and PageSize between 1 and 200.");
            return false;
        }

        return true;
    }
}

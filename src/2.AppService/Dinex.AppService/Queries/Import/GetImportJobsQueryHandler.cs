namespace Dinex.AppService;

public class GetImportJobsQueryHandler :
    IRequestHandler<GetImportJobsQuery, OperationResult<IEnumerable<ImportJobListItemDTO>>>,
    IQueryHandler
{
    private readonly IImportJobRepository _repo;

    public GetImportJobsQueryHandler(IImportJobRepository repo)
    {
        _repo = repo;
    }

    public async Task<OperationResult<IEnumerable<ImportJobListItemDTO>>> Handle(
        GetImportJobsQuery request,
        CancellationToken cancellationToken)
    {
        var result = new OperationResult<IEnumerable<ImportJobListItemDTO>>();

        var items = await _repo.GetAllAsync();

        // Se vocês usam deleção lógica, filtrar aqui:
        // items = items.Where(x => x.DeletedAt == null);

        // filtro por status (string → enum)
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<ImportJobStatus>(request.Status, ignoreCase: true, out var status))
        {
            items = items.Where(x => x.Status == status);
        }

        // ordena do mais novo
        items = items.OrderByDescending(x => x.UploadedAt);

        // paginação simples (opcional)
        if (request.Page.HasValue && request.PageSize.HasValue && request.Page > 0 && request.PageSize > 0)
        {
            items = items
                .Skip(((int)request.Page - 1) * (int)request.PageSize)
                .Take((int)request.PageSize);
        }

        var dtos = items.Select(x =>
        {
            var total = x.TotalRows ?? 0;
            var errors = x.ErrorRows ?? 0;
            var imported = Math.Max(0, total - errors);

            return new ImportJobListItemDTO
            {
                Id = x.Id,
                FileName = x.FileName,
                UploadedAt = x.UploadedAt,
                Status = x.Status.ToString(),
                TotalRows = x.TotalRows,
                ImportedRows = imported,
                ErrorsCount = errors,
                PeriodStartUtc = x.PeriodStartUtc.GetValueOrDefault(),
                PeriodEndUtc = x.PeriodEndUtc.GetValueOrDefault()
            };
        });

        result.SetData(dtos);
        return result;
    }
}

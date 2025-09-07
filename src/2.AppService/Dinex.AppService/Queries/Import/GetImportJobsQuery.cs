namespace Dinex.AppService;

public record GetImportJobsQuery(
    string? Status = null,   // opcional: "Pendente|Processando|Concluido|Falha"
    int? Page = null,
    int? PageSize = null
) : IRequest<OperationResult<IEnumerable<ImportJobListItemDTO>>>, IQueryHandler;

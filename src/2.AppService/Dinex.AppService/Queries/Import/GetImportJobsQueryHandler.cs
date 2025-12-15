using Dinex.Core;

namespace Dinex.AppService;

public sealed class GetImportJobsQueryHandler
    : IRequestHandler<GetImportJobsQuery, OperationResult<IEnumerable<ImportJobListItemDTO>>>
    , IQueryHandler
{
    private readonly IImportJobRepository _jobRepo;
    private readonly IB3StatementRowRepository _rowRepo;

    public GetImportJobsQueryHandler(
        IImportJobRepository jobRepo,
        IB3StatementRowRepository rowRepo)
    {
        _jobRepo = jobRepo;
        _rowRepo = rowRepo;
    }

        public async Task<OperationResult<IEnumerable<ImportJobListItemDTO>>> Handle(
            GetImportJobsQuery request,
            CancellationToken cancellationToken)
        {
            var result = new OperationResult<IEnumerable<ImportJobListItemDTO>>();

            // 1) Jobs (como já era)
            var items = await _jobRepo.GetAllAsync();

            // Se vocês usam deleção lógica, filtrar aqui:
            // items = items.Where(x => x.DeletedAt == null);

            // 2) filtro por status (string → enum)
            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<ImportJobStatus>(request.Status, ignoreCase: true, out var status))
            {
                items = items.Where(x => x.Status == status);
            }

            // 3) ordena do mais novo
            items = items.OrderByDescending(x => x.UploadedAt);

            // 4) paginação em memória (igual já faz hoje)
            if (request.Page.HasValue && request.PageSize.HasValue &&
                request.Page > 0 && request.PageSize > 0)
            {
                items = items
                    .Skip(((int)request.Page - 1) * (int)request.PageSize)
                    .Take((int)request.PageSize);
            }

            var jobsPage = items.ToList();

            // 5) para cada job, buscar as linhas e calcular métricas de trade
            var dtoList = new List<ImportJobListItemDTO>();

            foreach (var job in jobsPage)
            {
                // pergunta pro repositório de linhas
                var rows = await _rowRepo.GetByImportJobIdAsync(job.Id);

                var total = job.TotalRows ?? 0;
                var errors = job.ErrorRows ?? 0;
                var imported = Math.Max(0, total - errors);

                // só RV trade (a regra que estamos processando hoje)
                var tradeRows = rows.Where(r =>
                        r.StatementCategory == StatementCategory.TradeBuy ||
                        r.StatementCategory == StatementCategory.TradeSell)
                    .ToList();

                var totalTrades = tradeRows.Count;
                var processedTrades = tradeRows.Count(r => r.ProcessedTrade == true);

                // pendente “processável”
                var pendingTrades = tradeRows.Count(r =>
                    r.ProcessedTrade != true &&
                    r.Status != B3StatementRowStatus.Erro); // ajuste pro teu enum

                // erros de trade (pra você exibir separado se quiser)
                var tradeErrorRows = tradeRows.Count(r => r.Status == B3StatementRowStatus.Erro);

                var dto = new ImportJobListItemDTO
                {
                    Id = job.Id,
                    FileName = job.FileName,
                    UploadedAt = job.UploadedAt,
                    Status = job.Status.ToString(),

                    TotalRows = job.TotalRows,
                    ImportedRows = imported,
                    ErrorsCount = errors,
                    PeriodStartUtc = job.PeriodStartUtc.GetValueOrDefault(),
                    PeriodEndUtc = job.PeriodEndUtc.GetValueOrDefault(),

                    // se você criar esses campos no DTO, dá pra alimentar o front:
                    TotalTradeRows = totalTrades,
                    ProcessedTradeRows = processedTrades,
                    RemainingTradeRows = pendingTrades,
                    TradeErrorRows = tradeErrorRows // novo campo opcional
                };

                dtoList.Add(dto);
            }

            result.SetData(dtoList);
            return result;
        }
}

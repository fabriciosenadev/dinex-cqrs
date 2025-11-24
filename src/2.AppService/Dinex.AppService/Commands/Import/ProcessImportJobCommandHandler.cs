namespace Dinex.AppService;

public sealed class ProcessImportJobCommandHandler
    : IRequestHandler<ProcessImportJobCommand, OperationResult<ProcessReport>>
    , IQueryHandler
{
    private readonly IImportJobRepository _jobRepo;
    private readonly IB3StatementRowRepository _rowRepo;
    private readonly IAssetRepository _assetRepo;
    private readonly IOperationRepository _opRepo;
    private readonly IBrokerRepository _brokerRepo; // ⬅ novo

    public ProcessImportJobCommandHandler(
        IImportJobRepository jobRepo,
        IB3StatementRowRepository rowRepo,
        IAssetRepository assetRepo,
        IOperationRepository opRepo,
        IBrokerRepository brokerRepo) // ⬅ injeta aqui
    {
        _jobRepo = jobRepo;
        _rowRepo = rowRepo;
        _assetRepo = assetRepo;
        _opRepo = opRepo;
        _brokerRepo = brokerRepo;
    }

    public async Task<OperationResult<ProcessReport>> Handle(
        ProcessImportJobCommand request,
        CancellationToken ct)
    {
        var result = new OperationResult<ProcessReport>();

        try
        {
            // ===== Guard clauses (simples) =====
            if (request.ImportJobId == Guid.Empty)
                return result.AddError("ImportJobId é obrigatório.");

            if (request.WalletId == Guid.Empty)
                return result.AddError("WalletId é obrigatório.");

            if (!Enum.IsDefined(typeof(BrokerResolutionMode), request.BrokerMode))
                return result.AddError("BrokerMode inválido.");

            if (request.BrokerMode == BrokerResolutionMode.FromScreen &&
                (request.BrokerId is null || request.BrokerId == Guid.Empty))
            {
                return result.AddError("BrokerId é obrigatório quando BrokerMode = FromScreen.");
            }

            // ===== Carrega job e linhas =====
            var job = await _jobRepo.GetByIdAsync(request.ImportJobId);
            if (job is null)
                return result.SetAsNotFound().AddError("ImportJob não encontrado.");

            var allRows = await _rowRepo.GetByImportJobIdAsync(request.ImportJobId);

            // Filtra só linhas válidas e só Trades
            var rows = allRows
                .Where(r => r.Status != B3StatementRowStatus.Erro)
                .Where(r => r.StatementCategory == StatementCategory.TradeBuy
                         || r.StatementCategory == StatementCategory.TradeSell)
                .OrderBy(r => r.RowNumber)
                .ToList();

            var report = new ProcessReport
            {
                ImportJobId = request.ImportJobId,
                StartedAt = DateTime.UtcNow
            };

            // ===== Loop principal (KISS) =====
            foreach (var row in rows)
            {
                try
                {
                    await ProcessTradeAsync(row, request);
                    report.Processed++;
                }
                catch
                {
                    report.Errors++;
                    // Não relança – seguimos para a próxima linha.
                }
            }

            // Linhas ignoradas = total - processadas - erros
            var totalTradeRows = rows.Count;
            report.Skipped = Math.Max(0, totalTradeRows - report.Processed - report.Errors);
            report.FinishedAt = DateTime.UtcNow;

            return result.SetData(report);
        }
        catch (Exception ex)
        {
            return result.AddError(ex.Message).SetAsInternalServerError();
        }
    }

    // =========================
    // Trades (direto ao ponto)
    // =========================
    private async Task ProcessTradeAsync(
        B3StatementRow row,
        ProcessImportJobCommand request)
    {
        // 1) Wallet: fixa por request (obrigatória)
        var walletId = request.WalletId;

        // 2) Broker
        var brokerId = await GetBrokerIdAsync(row, request);

        // 3) Asset: pega ou cria
        var assetId = await GetOrCreateAssetIdAsync(row);
        if (assetId == Guid.Empty)
            throw new InvalidOperationException("Não foi possível resolver o ativo para a linha.");

        // 4) Operation type
        var opType = row.OperationType
                     ?? (row.StatementCategory == StatementCategory.TradeSell
                         ? OperationType.Sell
                         : OperationType.Buy);

        // 5) Cria Operation
        var op = Operation.Create(
            walletId: walletId,
            assetId: assetId,
            type: opType,
            quantity: row.Quantity!.Value,
            unitPrice: row.UnitPrice!.Value,
            executedAt: EnsureUtc(row.Date),
            brokerId: brokerId
        );

        if (!op.IsValid)
        {
            var msg = string.Join("; ", op.Notifications.Select(n => n.Message));
            throw new InvalidOperationException(msg);
        }

        await _opRepo.AddAsync(op);
        await _opRepo.SaveChangesAsync();
    }

    // =========================
    // Helpers de Broker
    // =========================
    private async Task<Guid?> GetBrokerIdAsync(
        B3StatementRow row,
        ProcessImportJobCommand request)
    {
        if (request.BrokerMode == BrokerResolutionMode.FromScreen)
            return request.BrokerId;

        // FromFile
        var raw = row.Broker;
        if (string.IsNullOrWhiteSpace(raw))
            return null; // se quiser, pode jogar exception aqui

        var normalized = NormalizeBroker(raw);

        // 1) tenta achar
        var existing = await _brokerRepo.GetByNormalizedNameAsync(normalized);
        if (existing is not null)
            return existing.Id;

        // 2) não achou → cria
        var newBroker = Broker.Create(
            name: normalized,
            cnpj: null,
            website: null
        );

        await _brokerRepo.AddAsync(newBroker);
        await _brokerRepo.SaveChangesAsync();

        return newBroker.Id;
    }

    // =========================
    // Helpers de Asset
    // =========================
    private async Task<Guid> GetOrCreateAssetIdAsync(
        B3StatementRow row)
    {
        var raw = row.Asset;
        if (string.IsNullOrWhiteSpace(raw))
            return Guid.Empty;

        var ticker = ExtractTicker(raw);
        if (string.IsNullOrWhiteSpace(ticker))
            return Guid.Empty;

        var existing = await _assetRepo.GetByCodeAsync(ticker);
        if (existing is not null)
            return existing.Id;

        // cria um asset "mínimo" – ajuste conforme sua entidade
        var asset = Asset.Create(
            name: ticker,
            code: ticker,
            cnpj: null,
            exchange: Exchange.B3,       // ou outro default
            currency: Currency.BRL,      // ajuste se quiser
            type: AssetType.Acao,        // ou inferir de outro lugar
            sector: null
        );

        if (!asset.IsValid)
        {
            var msg = string.Join("; ", asset.Notifications.Select(n => n.Message));
            throw new InvalidOperationException(msg);
        }

        await _assetRepo.AddAsync(asset);
        await _assetRepo.SaveChangesAsync();

        return asset.Id;
    }

    // =========================
    // Helpers simples
    // =========================
    private static string NormalizeBroker(string? s) =>
        (s ?? string.Empty).Trim().ToUpperInvariant();

    private static string ExtractTicker(string rawAsset)
    {
        if (string.IsNullOrWhiteSpace(rawAsset)) return string.Empty;
        var primary = rawAsset.Split('-', ' ', '·').FirstOrDefault() ?? "";
        return primary.Trim().ToUpperInvariant();
    }

    private static DateTime EnsureUtc(DateTime dt) =>
        DateTime.SpecifyKind(dt, DateTimeKind.Utc);
}

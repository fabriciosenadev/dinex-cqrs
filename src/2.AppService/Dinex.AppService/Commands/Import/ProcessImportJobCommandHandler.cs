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
    private readonly IPositionService _positionService;

    static readonly HashSet<StatementCategory> ProcessableCategories = new()
    {
        StatementCategory.TradeBuy,
        StatementCategory.TradeSell,

        // eventos que alteram quantidade sem caixa
        StatementCategory.CorpActionSplit,
        StatementCategory.CorpActionBonus,
        StatementCategory.CorpActionReverseSplit,

        // ajustes/posição
        StatementCategory.PositionAdjustment,
        StatementCategory.PositionFraction,

        // se quiser tentar depois (eu deixaria pra fase 2)
        // StatementCategory.CorpActionIncorporation,
    };

    public ProcessImportJobCommandHandler(
        IImportJobRepository jobRepo,
        IB3StatementRowRepository rowRepo,
        IAssetRepository assetRepo,
        IOperationRepository opRepo,
        IBrokerRepository brokerRepo,
        IPositionService positionService) // ⬅ injeta aqui
    {
        _jobRepo = jobRepo;
        _rowRepo = rowRepo;
        _assetRepo = assetRepo;
        _opRepo = opRepo;
        _brokerRepo = brokerRepo;
        _positionService = positionService;
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
                .Where(r => r.ProcessedTrade != true) // ok por enquanto, apesar do nome ruim
                .Where(r => ProcessableCategories.Contains(r.StatementCategory))
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
                    await ProcessRowAsync(row, request);

                    row.MarkTradeProcessed(); // por enquanto, reutiliza
                    await _rowRepo.UpdateAsync(row);

                    report.Processed++;
                }
                catch (Exception ex)
                {
                    row.MarkAsError(ex.Message);
                    await _rowRepo.UpdateAsync(row);

                    report.Errors++;
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

    private async Task ProcessRowAsync(B3StatementRow row, ProcessImportJobCommand request)
    {
        switch (row.StatementCategory)
        {
            case StatementCategory.TradeBuy:
            case StatementCategory.TradeSell:
                await ProcessTradeAsync(row, request);
                return;

            case StatementCategory.CorpActionSplit:
            case StatementCategory.CorpActionBonus:
            case StatementCategory.CorpActionReverseSplit:
            case StatementCategory.PositionAdjustment:
            case StatementCategory.PositionFraction:
                await ProcessNoCashPositionChangeAsync(row, request);
                return;

            default:
                // defensivo: se passou no filtro, não deveria cair aqui
                throw new InvalidOperationException($"Categoria não suportada: {row.StatementCategory}");
        }
    }

    private async Task ProcessTradeAsync(
        B3StatementRow row,
        ProcessImportJobCommand request)
    {
        // ===== Guard rail 1: renda fixa nunca vira Operation =====
        if (LooksLikeFixedIncome(row.Asset))
            throw new InvalidOperationException($"Linha {row.RowNumber}: ativo parece Renda Fixa, não é trade de RV.");

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

        // 5) sanity check ticker (opcional, mas recomendado)
        var ticker = ExtractTicker(row.Asset!);
        if (!LooksLikeEquityTicker(ticker))
            throw new InvalidOperationException($"Linha {row.RowNumber}: ticker inválido para RV ('{ticker}').");

        // 6) valida campos numéricos essenciais
        if (row.Quantity is null || row.UnitPrice is null)
            throw new InvalidOperationException($"Linha {row.RowNumber}: Quantity/UnitPrice ausentes.");

        var op = Operation.Create(
            walletId: walletId,
            assetId: assetId,
            type: opType,
            quantity: row.Quantity.Value,
            unitPrice: row.UnitPrice.Value,
            executedAt: EnsureUtc(row.Date),
            brokerId: brokerId
        );

        if (!op.IsValid)
        {
            var msg = string.Join("; ", op.Notifications.Select(n => n.Message));
            throw new InvalidOperationException(msg);
        }

        await _opRepo.AddAsync(op);

        await _positionService.RecalculatePositionAsync(walletId, assetId, op);
    }

    private async Task ProcessNoCashPositionChangeAsync(
        B3StatementRow row,
        ProcessImportJobCommand request)
    {
        // wallet fixa
        var walletId = request.WalletId;

        // asset resolve igual ao trade, mas normalizando o ticker
        var ticker = ExtractTickerNormalized(row.Asset!);
        if (!LooksLikeEquityTicker(ticker))
            throw new InvalidOperationException($"Linha {row.RowNumber}: ticker inválido ('{ticker}').");

        // resolve/garante Asset
        var existing = await _assetRepo.GetByCodeAsync(ticker);
        Guid assetId;
        if (existing is not null) assetId = existing.Id;
        else
        {
            var asset = Asset.Create(
                name: ticker,
                code: ticker,
                cnpj: null,
                exchange: Exchange.B3,
                currency: Currency.BRL,
                type: AssetType.Acao,
                sector: null
            );

            if (!asset.IsValid)
                throw new InvalidOperationException(string.Join("; ", asset.Notifications.Select(n => n.Message)));

            await _assetRepo.AddAsync(asset);
            assetId = asset.Id;
        }

        if (row.Quantity is null || row.Quantity.Value <= 0)
            throw new InvalidOperationException($"Linha {row.RowNumber}: Quantity ausente/inválida para ajuste de posição.");

        // aplica delta conforme categoria + ledgerSide
        decimal delta = row.Quantity.Value;

        // regra simples: Crédito soma, Débito subtrai
        if (row.LedgerSide == LedgerSide.Debit)
            delta = -delta;

        // alguns eventos são sempre “crédito” na prática (bonus/split),
        // mas manter a regra por ledgerSide te protege.
        await _positionService.ApplyQuantityDeltaNoCashAsync(walletId, assetId, delta, row);
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

    private static bool LooksLikeFixedIncome(string? asset)
    {
        if (string.IsNullOrWhiteSpace(asset)) return false;
        var a = asset.Trim().ToUpperInvariant();

        // mesma regra do classificador (mínimo necessário)
        return a.StartsWith("CDB ") || a.StartsWith("CDB -")
            || a.StartsWith("LCI ") || a.StartsWith("LCI -")
            || a.StartsWith("LCA ") || a.StartsWith("LCA -")
            || a.StartsWith("LC ") || a.StartsWith("LC -")
            || a.StartsWith("CRI ") || a.StartsWith("CRI -")
            || a.StartsWith("CRA ") || a.StartsWith("CRA -")
            || a.Contains("DEBENTURE")
            || a.Contains("TESOURO")
            || a.Contains("LTN") || a.Contains("LFT") || a.Contains("NTN");
    }

    private static bool LooksLikeEquityTicker(string? ticker)
    {
        if (string.IsNullOrWhiteSpace(ticker)) return false;
        // AAAA3 / AAAA11 / AAAA3F
        return Regex.IsMatch(ticker.Trim().ToUpperInvariant(), @"^[A-Z]{4}[0-9]{1,2}F?$");
    }

    private static string ExtractTickerNormalized(string rawAsset)
    {
        var t = ExtractTicker(rawAsset);

        // fracionário AAAA3F -> AAAA3 (posição na B3 geralmente consolida no ativo principal)
        if (t.EndsWith("F", StringComparison.OrdinalIgnoreCase))
            t = t[..^1];

        return t;
    }
}

namespace Dinex.AppService;

public interface IB3StatementClassifier
{
    (StatementCategory category, OperationType? tradeSide) Classify(
        string? movement,
        string? asset,
        LedgerSide? ledgerSide,
        decimal? quantity,
        decimal? unitPrice,
        decimal? totalValue);
}

public class B3StatementClassifier : IB3StatementClassifier
{
    // Regex para identificar tickers de renda variável (AAAA3, AAAA4, AAAA11, etc).
    private static readonly Regex RendaVariavelTickerRx =
        new(@"^[A-Z]{4}[0-9]{1,2}$", RegexOptions.Compiled);

    private static bool IsRendaVariavel(string? asset)
    {
        if (string.IsNullOrWhiteSpace(asset)) return false;

        var primary = asset.Split('-', ' ', '·')
                           .FirstOrDefault()?
                           .Trim()
                           .ToUpperInvariant();

        return !string.IsNullOrWhiteSpace(primary)
            && RendaVariavelTickerRx.IsMatch(primary);
    }

    public (StatementCategory category, OperationType? tradeSide) Classify(
        string? movement,
        string? asset,
        LedgerSide? ledgerSide,
        decimal? quantity,
        decimal? unitPrice,
        decimal? totalValue)
    {
        var mov = (movement ?? "").Trim().ToUpperInvariant();
        bool isRV = IsRendaVariavel(asset);

        // ==========================================================
        // 1) TRADE (RENA VARIÁVEL COM REGRAS SIMPLIFICADAS)
        // ==========================================================

        if (isRV)
        {
            // --- A) Transferência - Liquidação → compra/venda ---
            if (mov.Contains("TRANSFERÊNCIA - LIQUIDAÇÃO") ||
                mov.Contains("TRANSFERENCIA - LIQUIDACAO"))
            {
                return ledgerSide switch
                {
                    LedgerSide.Credit => (StatementCategory.TradeBuy, OperationType.Buy),
                    LedgerSide.Debit => (StatementCategory.TradeSell, OperationType.Sell),
                    _ => (StatementCategory.Unknown, null)
                };
            }

            // --- B) Compra explícita ---
            if (mov.Contains("COMPRA") || mov.Contains("C/VISTA") || mov.Contains("C VISTA"))
                return (StatementCategory.TradeBuy, OperationType.Buy);

            // --- C) Venda explícita ---
            if (mov.Contains("VENDA") || mov.Contains("V/VISTA") || mov.Contains("V VISTA"))
                return (StatementCategory.TradeSell, OperationType.Sell);
        }

        // ==========================================================
        // 2) TUDO QUE NÃO FOR TRADE → SEM CLASSIFICAÇÃO POR HORA
        // ==========================================================
        return (StatementCategory.Unknown, null);
    }
}

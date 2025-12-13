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
    // encontra AAAA3, AAAA11, e também AAAA3F (fracionário)
    private static readonly Regex RendaVariavelTickerRx =
        new(@"\b[A-Z0-9]{4,6}[0-9]{1,2}F?\b", RegexOptions.Compiled);

    private static bool IsRendaVariavel(string? asset)
    {
        if (string.IsNullOrWhiteSpace(asset)) return false;

        var normalized = Normalize(asset);
        return RendaVariavelTickerRx.IsMatch(normalized);
    }


    // reaproveita o mesmo Normalize do parser (ideal: extrair para helper compartilhado)
    private static string Normalize(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        s = s.Trim().ToUpperInvariant();
        var normalized = s.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
    }

    public (StatementCategory category, OperationType? tradeSide) Classify(
        string? movement,
        string? asset,
        LedgerSide? ledgerSide,
        decimal? quantity,
        decimal? unitPrice,
        decimal? totalValue)
    {
        var mov = Normalize(movement);
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

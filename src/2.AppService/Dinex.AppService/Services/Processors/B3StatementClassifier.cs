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
    // Mapeamento por palavra-chave em MOVIMENTO **normalizado** (maiúsculas, sem acento)
    private static readonly (Regex rx, StatementCategory cat)[] Map = new[]
    {
        // === Ajustes/Posição (sem caixa) ===
        (Rx(@"^FRACAO\s+EM\s+ATIVOS?"), StatementCategory.PositionFraction),    // "Fração em Ativos"
        (Rx(@"^ATUALIZA"),                 StatementCategory.PositionAdjustment), // "Atualização"

        // === Proventos ===
        (Rx("DIVIDEND"),                   StatementCategory.CashIncomeDividend),
        (Rx("DIVIDENDO"),                  StatementCategory.CashIncomeDividend),
        (Rx("JCP|JUROS SOBRE CAPITAL"),    StatementCategory.CashIncomeJCP),
        (Rx("RENDIMENTO"),                 StatementCategory.CashIncomeFII),
        (Rx("AMORTIZA"),                   StatementCategory.CashIncomeAmortization),

        // === Eventos corporativos (sem caixa) ===
        (Rx("BONIFIC"),                    StatementCategory.CorpActionBonus),
        (Rx("DESDOBRA"),                   StatementCategory.CorpActionSplit),
        (Rx("GRUPA"),                      StatementCategory.CorpActionReverseSplit),
        (Rx("INCORPORA"),                  StatementCategory.CorpActionIncorporation),
        (Rx("SUBSCRI|DIREITO|EXERCICIO"),  StatementCategory.RightsSubscription),

        // === Transfer / Taxas ===
        (Rx("TRANSFER"),                   StatementCategory.TransferIn), // direção ajustada abaixo por ledger side
        (Rx("TAXA|EMOLUM|CORRETAG|ISS|IOF|IRRF"), StatementCategory.TaxOrFee),
    };

    public (StatementCategory category, OperationType? tradeSide) Classify(
        string? movement,
        string? asset,
        LedgerSide? ledgerSide,
        decimal? quantity,
        decimal? unitPrice,
        decimal? totalValue)
    {
        // Normaliza: MAIÚSCULAS + remove diacríticos (FRAÇÃO -> FRACAO)
        var mov = Normalize(movement);

        // 1) Map por palavra-chave
        foreach (var (rx, cat) in Map)
        {
            if (rx.IsMatch(mov))
            {
                // Transferências: ajusta In/Out conforme ledger side
                if (cat == StatementCategory.TransferIn)
                {
                    if (ledgerSide == Core.LedgerSide.Credit) return (StatementCategory.TransferIn, null);
                    if (ledgerSide == Core.LedgerSide.Debit) return (StatementCategory.TransferOut, null);
                    return (StatementCategory.TransferIn, null);
                }

                // Para PositionFraction/PositionAdjustment (e demais não-trade), tradeSide = null
                return (cat, null);
            }
        }

        // 2) Heurística para trades (Q>0, PU>=0, Total>=0)
        var hasQP = quantity is > 0 && unitPrice is >= 0 && totalValue is >= 0;
        if (hasQP)
        {
            if (ledgerSide == Core.LedgerSide.Debit) return (StatementCategory.TradeSell, OperationType.Sell);
            if (ledgerSide == Core.LedgerSide.Credit) return (StatementCategory.TradeBuy, OperationType.Buy);
            return (StatementCategory.TradeBuy, OperationType.Buy);
        }

        // 3) Heurística para provento (somente TotalValue > 0)
        if ((totalValue ?? 0) > 0 && (quantity is null || quantity == 0) && unitPrice is null)
        {
            if (!string.IsNullOrWhiteSpace(asset) && asset.Trim().ToUpperInvariant().EndsWith("11"))
                return (StatementCategory.CashIncomeFII, null);

            return (StatementCategory.CashIncomeDividend, null);
        }

        return (StatementCategory.Unknown, null);
    }

    private static Regex Rx(string pattern) =>
        new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static string Normalize(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        s = s.Trim().ToUpperInvariant();

        var normalized = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark) sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC); // ex.: "FRAÇÃO" -> "FRACAO"
    }
}

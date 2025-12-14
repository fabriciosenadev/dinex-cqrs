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
    // encontra AAAA3, AAAA11, e também AAAA3F (fracionário).
    // Obs: não usamos isso como "gate" para proventos/eventos; serve mais como heurística de ticker.
    private static readonly Regex RendaVariavelTickerRx =
        new(@"\b[A-Z0-9]{4,6}[0-9]{1,2}F?\b", RegexOptions.Compiled);

    private static bool LooksLikeTicker(string? asset)
    {
        if (string.IsNullOrWhiteSpace(asset)) return false;
        var normalized = Normalize(asset);
        return RendaVariavelTickerRx.IsMatch(normalized);
    }

    // remove acentos e normaliza caixa/espacos
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

        // remove espaços duplicados também ajuda em alguns extratos
        return Regex.Replace(sb.ToString(), @"\s+", " ")
            .Normalize(System.Text.NormalizationForm.FormC);
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

        // ==========================================================
        // 0) Guard rails
        // ==========================================================
        // Se não tem movimento, não tenta adivinhar
        if (string.IsNullOrWhiteSpace(mov))
            return (StatementCategory.Unknown, null);

        // ==========================================================
        // 1) PROVENTOS (cash income) — vem antes de trade
        // ==========================================================
        if (mov.Contains("DIVIDENDO") || mov.Contains("DIVIDEND"))
            return (StatementCategory.CashIncomeDividend, null);

        if (mov.Contains("JCP") || mov.Contains("JUROS SOBRE CAPITAL"))
            return (StatementCategory.CashIncomeJCP, null);

        // "RENDIMENTO" costuma ser FII, mas se vier em ações o usuário ajusta depois
        if (mov.Contains("RENDIMENTO"))
            return (StatementCategory.CashIncomeFII, null);

        if (mov.Contains("AMORTIZA"))
            return (StatementCategory.CashIncomeAmortization, null);

        // ==========================================================
        // 2) EVENTOS SEM CAIXA (corporate actions) — afetam quantidade
        // ==========================================================
        if (mov.Contains("BONIFIC"))
            return (StatementCategory.CorpActionBonus, null);

        if (mov.Contains("DESDOBRA"))
            return (StatementCategory.CorpActionSplit, null);

        if (mov.Contains("GRUPA"))
            return (StatementCategory.CorpActionReverseSplit, null);

        // "INCORPORACAO" / "INCORPORA" (sem acento)
        if (mov.Contains("INCORPORA"))
            return (StatementCategory.CorpActionIncorporation, null);

        // direitos/subscrição
        if (mov.Contains("DIREITO") || mov.Contains("SUBSCR"))
            return (StatementCategory.RightsSubscription, null);

        // ==========================================================
        // 3) AJUSTES / ATUALIZAÇÕES DE POSIÇÃO
        // ==========================================================
        if (mov.Contains("FRACAO"))
            return (StatementCategory.PositionFraction, null);

        if (mov.Contains("ATUALIZACAO") || mov.Contains("REAJUSTE"))
            return (StatementCategory.PositionAdjustment, null);

        // ==========================================================
        // 4) TAXAS / TARIFAS
        // ==========================================================
        // cobre "EMOLUMENTOS", "TAXA", "TARIFA", "CORRETAGEM", "ISS"
        if (mov.Contains("EMOLUMENT") ||
            mov.Contains("TAXA") ||
            mov.Contains("TARIFA") ||
            mov.Contains("CORRETAG") ||
            mov.Contains("ISS"))
        {
            return (StatementCategory.TaxOrFee, null);
        }

        // ==========================================================
        // 5) TRANSFERÊNCIAS (não-trade) VS "TRANSFERENCIA - LIQUIDACAO" (trade)
        // ==========================================================
        // IMPORTANTE: mov já está normalizado sem acento
        if (mov.Contains("TRANSFERENCIA - LIQUIDACAO"))
        {
            // Heurística principal do extrato B3:
            // Crédito -> compra (entrada de ativo), Débito -> venda (saída de ativo)
            return ledgerSide switch
            {
                LedgerSide.Credit => (StatementCategory.TradeBuy, OperationType.Buy),
                LedgerSide.Debit => (StatementCategory.TradeSell, OperationType.Sell),
                _ => (StatementCategory.Unknown, null)
            };
        }

        if (mov.Contains("TRANSFERENCIA"))
        {
            return ledgerSide switch
            {
                LedgerSide.Credit => (StatementCategory.TransferIn, null),
                LedgerSide.Debit => (StatementCategory.TransferOut, null),
                _ => (StatementCategory.Unknown, null)
            };
        }

        // ==========================================================
        // 6) TRADE explícito (por último)
        // ==========================================================
        // Aqui é OK usar heurística de ticker, mas não é obrigatório.
        // Se vier "COMPRA" / "VENDA" sem ticker, ainda assim provavelmente é trade,
        // porém manter o LooksLikeTicker reduz falsos positivos em descrições soltas.
        var looksLikeTicker = LooksLikeTicker(asset);

        if (mov.Contains("COMPRA") || mov.Contains("C/VISTA") || mov.Contains("C VISTA"))
        {
            return looksLikeTicker
                ? (StatementCategory.TradeBuy, OperationType.Buy)
                : (StatementCategory.TradeBuy, OperationType.Buy);
        }

        if (mov.Contains("VENDA") || mov.Contains("V/VISTA") || mov.Contains("V VISTA"))
        {
            return looksLikeTicker
                ? (StatementCategory.TradeSell, OperationType.Sell)
                : (StatementCategory.TradeSell, OperationType.Sell);
        }

        // ==========================================================
        // fallback
        // ==========================================================
        return (StatementCategory.Unknown, null);
    }
}

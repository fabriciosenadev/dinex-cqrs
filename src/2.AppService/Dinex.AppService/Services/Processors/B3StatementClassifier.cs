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
    private static readonly Regex RendaVariavelTickerRx =
        new(@"\b[A-Z]{4}[0-9]{1,2}F?\b", RegexOptions.Compiled);

    private static bool LooksLikeTicker(string? asset)
    {
        if (string.IsNullOrWhiteSpace(asset)) return false;
        var normalized = Normalize(asset);
        return RendaVariavelTickerRx.IsMatch(normalized);
    }

    // guard rail: renda fixa (CDB/LCI/LCA etc)
    private static bool LooksLikeFixedIncome(string? asset)
    {
        if (string.IsNullOrWhiteSpace(asset)) return false;

        var a = Normalize(asset);

        // exemplos reais teus:
        // "CDB - CDB223TZK4R - ...", "LCI - 19B00199675 - ..."
        // então o prefixo é o que manda.
        if (a.StartsWith("CDB ") || a.StartsWith("CDB -")) return true;
        if (a.StartsWith("LCI ") || a.StartsWith("LCI -")) return true;
        if (a.StartsWith("LCA ") || a.StartsWith("LCA -")) return true;
        if (a.StartsWith("LC ") || a.StartsWith("LC -")) return true;

        // alguns comuns de RF que podem aparecer no futuro
        if (a.StartsWith("CRI ") || a.StartsWith("CRI -")) return true;
        if (a.StartsWith("CRA ") || a.StartsWith("CRA -")) return true;
        if (a.StartsWith("DEB ") || a.StartsWith("DEB -")) return true;
        if (a.Contains("DEBENTURE")) return true;

        // tesouro/ títulos públicos (se algum extrato vier assim)
        if (a.Contains("TESOURO")) return true;
        if (a.Contains("LTN") || a.Contains("LFT") || a.Contains("NTN")) return true;

        return false;
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
        if (string.IsNullOrWhiteSpace(mov))
            return (StatementCategory.Unknown, null);

        var isFixedIncome = LooksLikeFixedIncome(asset);
        var looksLikeTicker = LooksLikeTicker(asset);

        // ==========================================================
        // 1) PROVENTOS (cash income) — vem antes de trade
        // ==========================================================
        if (mov.Contains("DIVIDENDO") || mov.Contains("DIVIDEND"))
            return (StatementCategory.CashIncomeDividend, null);

        if (mov.Contains("JCP") || mov.Contains("JUROS SOBRE CAPITAL"))
            return (StatementCategory.CashIncomeJCP, null);

        if (mov.Contains("RENDIMENTO"))
            return (StatementCategory.CashIncomeFII, null);

        if (mov.Contains("AMORTIZA"))
            return (StatementCategory.CashIncomeAmortization, null);

        // ==========================================================
        // 2) EVENTOS SEM CAIXA (corporate actions)
        // ==========================================================
        if (mov.Contains("BONIFIC"))
            return (StatementCategory.CorpActionBonus, null);

        if (mov.Contains("DESDOBRA"))
            return (StatementCategory.CorpActionSplit, null);

        if (mov.Contains("GRUPA"))
            return (StatementCategory.CorpActionReverseSplit, null);

        if (mov.Contains("INCORPORA"))
            return (StatementCategory.CorpActionIncorporation, null);

        if (mov.Contains("DIREITO") || mov.Contains("SUBSCR"))
            return (StatementCategory.RightsSubscription, null);

        // ==========================================================
        // 3) AJUSTES / POSIÇÃO
        // ==========================================================
        if (mov.Contains("FRACAO"))
            return (StatementCategory.PositionFraction, null);

        if (mov.Contains("ATUALIZACAO") || mov.Contains("REAJUSTE"))
            return (StatementCategory.PositionAdjustment, null);

        // ==========================================================
        // 4) TAXAS / TARIFAS
        // ==========================================================
        if (mov.Contains("EMOLUMENT") ||
            mov.Contains("TAXA") ||
            mov.Contains("TARIFA") ||
            mov.Contains("CORRETAG") ||
            mov.Contains("ISS"))
        {
            return (StatementCategory.TaxOrFee, null);
        }

        // ==========================================================
        // 5) TRANSFERÊNCIAS
        // ==========================================================
        // A) "TRANSFERENCIA - LIQUIDACAO" é RV trade na maioria dos casos,
        //    mas pode aparecer em RF (CDB/LCI/LCA). Guard rail: RF nunca vira Trade.
        if (mov.Contains("TRANSFERENCIA - LIQUIDACAO"))
        {
            // RF: NÃO É TRADE de RV
            if (isFixedIncome)
            {
                return ledgerSide switch
                {
                    LedgerSide.Credit => (StatementCategory.TransferIn, null),
                    LedgerSide.Debit => (StatementCategory.TransferOut, null),
                    _ => (StatementCategory.Unknown, null)
                };
            }

            // RV: só assume trade se tiver cara de ticker
            if (looksLikeTicker)
            {
                return ledgerSide switch
                {
                    LedgerSide.Credit => (StatementCategory.TradeBuy, OperationType.Buy),
                    LedgerSide.Debit => (StatementCategory.TradeSell, OperationType.Sell),
                    _ => (StatementCategory.Unknown, null)
                };
            }

            // sem ticker e sem RF → melhor não adivinhar
            return (StatementCategory.Unknown, null);
        }

        // B) outras transferências não-trade
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
        // Guard rail: RF nunca vira trade mesmo se o texto tiver "COMPRA"/"VENDA"
        if (!isFixedIncome)
        {
            if (mov.Contains("COMPRA") || mov.Contains("C/VISTA") || mov.Contains("C VISTA"))
            {
                // aqui pode manter sem gate rígido, mas se quiser reduzir falso positivo:
                // if (!looksLikeTicker) return (StatementCategory.Unknown, null);
                return (StatementCategory.TradeBuy, OperationType.Buy);
            }

            if (mov.Contains("VENDA") || mov.Contains("V/VISTA") || mov.Contains("V VISTA"))
            {
                // if (!looksLikeTicker) return (StatementCategory.Unknown, null);
                return (StatementCategory.TradeSell, OperationType.Sell);
            }
        }

        return (StatementCategory.Unknown, null);
    }
}

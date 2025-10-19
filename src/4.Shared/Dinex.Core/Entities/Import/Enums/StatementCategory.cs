namespace Dinex.Core;

public enum StatementCategory
{
    // Trades
    TradeBuy = 10,
    TradeSell = 11,

    // Proventos (cash income)
    CashIncomeDividend = 20,
    CashIncomeJCP = 21,
    CashIncomeFII = 22,
    CashIncomeAmortization = 23,

    // Eventos sem caixa (corporate actions)
    CorpActionBonus = 30,
    CorpActionSplit = 31,
    CorpActionReverseSplit = 32,
    CorpActionIncorporation = 33,
    RightsSubscription = 34,

    // Ajustes / atualizações de posição
    PositionFraction = 35,     // "Fração em Ativos"
    PositionAdjustment = 36,   // "Atualização" / "Reajuste de posição"

    // Transferências / taxas
    TransferIn = 40,
    TransferOut = 41,
    TaxOrFee = 42,

    // fallback
    Unknown = 99
}

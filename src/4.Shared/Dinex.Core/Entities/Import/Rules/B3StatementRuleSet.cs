namespace Dinex.Core;

public static class B3StatementRuleSet
{
    private const decimal TotalTolerance = 0.05m;

    /// <summary>
    /// Aplica validações semânticas baseadas em StatementCategory.
    /// Esta camada é isolada da entidade para permitir evolução sem refactor no parser.
    /// </summary>
    public static IEnumerable<string> Validate(B3StatementRow row)
    {
        var errors = new List<string>();

        switch (row.StatementCategory)
        {
            // ======================= TRADES =======================
            case StatementCategory.TradeBuy:
            case StatementCategory.TradeSell:
                Require(row.OperationType is not null, errors, "OperationType obrigatório para trades.");
                Require(row.Quantity is not null && row.Quantity > 0, errors, "Quantidade obrigatória e deve ser maior que zero para trades.");
                Require(row.UnitPrice is not null && row.UnitPrice >= 0, errors, "Preço Unitário obrigatório e não pode ser negativo para trades.");
                Require(row.TotalValue is not null && row.TotalValue >= 0, errors, "Valor Total obrigatório e não pode ser negativo para trades.");

                if (row.Quantity is not null && row.UnitPrice is not null && row.TotalValue is not null)
                {
                    var expected = Math.Round(row.Quantity.Value * row.UnitPrice.Value, 2, MidpointRounding.AwayFromZero);
                    var diff = Math.Abs(expected - row.TotalValue.Value);
                    if (diff > TotalTolerance)
                        errors.Add("Valor Total não confere com Quantidade × Preço Unitário (diferença acima de R$ 0,05).");
                }
                break;

            // =================== PROVENTOS EM DINHEIRO ===================
            case StatementCategory.CashIncomeDividend:
            case StatementCategory.CashIncomeJCP:
            case StatementCategory.CashIncomeFII:
            case StatementCategory.CashIncomeAmortization:
                Require(row.TotalValue is not null && row.TotalValue >= 0, errors, "Valor Total obrigatório e não pode ser negativo para proventos.");
                // Quantidade/Preço não são obrigatórios; se vierem, não podem ser negativos
                NotNegative(row.Quantity, errors, "Quantidade não pode ser negativa para proventos.");
                NotNegative(row.UnitPrice, errors, "Preço Unitário não pode ser negativo para proventos.");
                break;

            // =================== EVENTOS CORPORATIVOS S/ PREÇO ===================
            case StatementCategory.CorpActionBonus:
            case StatementCategory.CorpActionSplit:
            case StatementCategory.CorpActionReverseSplit:
                Require(row.Quantity is not null && row.Quantity > 0, errors, "Quantidade obrigatória (>0) para eventos de bonificação/desdobramento/grupamento.");
                // Preço/Valor não são obrigatórios; se vierem, não podem ser negativos
                NotNegative(row.UnitPrice, errors, "Preço Unitário não pode ser negativo para evento corporativo.");
                NotNegative(row.TotalValue, errors, "Valor Total não pode ser negativo para evento corporativo.");
                // OperationType não se aplica
                Require(row.OperationType is null, errors, "OperationType deve ser nulo para eventos corporativos sem negociação.");
                break;

            // =================== NOVOS CASOS ===================
            // Fração em Ativos -> ajuste de fração, sem fluxo financeiro
            case StatementCategory.PositionFraction:
                Require(row.Quantity is not null && row.Quantity > 0, errors, "Quantidade obrigatória (>0) para Fração em Ativos.");
                // Permitir decimais pequenos (ex.: 0.003). Preço/Valor opcionais, mas se vierem não podem ser negativos
                NotNegative(row.UnitPrice, errors, "Preço Unitário não pode ser negativo para Fração em Ativos.");
                // Valor financeiro normalmente é 0; se vier, não pode ser negativo
                Require(row.TotalValue is null || row.TotalValue >= 0, errors, "Valor Total não pode ser negativo para Fração em Ativos.");
                // OperationType não se aplica
                Require(row.OperationType is null, errors, "OperationType deve ser nulo para Fração em Ativos.");
                break;

            // Atualização -> ajuste/retificação contábil de posição (pode não alterar quantidade)
            case StatementCategory.PositionAdjustment:
                // Quantidade é opcional; quando vier, aceitaremos >= 0 (se precisar permitir negativos, troque para: row.Quantity is null || row.Quantity >= 0)
                Require(row.Quantity is null || row.Quantity >= 0, errors, "Quantidade não pode ser negativa para Atualização.");
                NotNegative(row.UnitPrice, errors, "Preço Unitário não pode ser negativo para Atualização.");
                Require(row.TotalValue is null || row.TotalValue >= 0, errors, "Valor Total não pode ser negativo para Atualização.");
                Require(row.OperationType is null, errors, "OperationType deve ser nulo para Atualização.");
                break;

            // =================== OUTROS ===================
            case StatementCategory.CorpActionIncorporation:
            case StatementCategory.RightsSubscription:
                // eventos societários complexos — podem requerer integração externa
                NotNegative(row.UnitPrice, errors, "Preço Unitário não pode ser negativo.");
                NotNegative(row.TotalValue, errors, "Valor Total não pode ser negativo.");
                break;

            case StatementCategory.TransferIn:
            case StatementCategory.TransferOut:
            case StatementCategory.TaxOrFee:
                // sem validação adicional específica aqui
                NotNegative(row.TotalValue, errors, "Valor Total não pode ser negativo.");
                break;

            case StatementCategory.Unknown:
            default:
                errors.Add("Movimentação não reconhecida para classificação automática.");
                break;
        }

        // Regras opcionais de integração (mantidas)
        if (ShouldCheckIntegration(row.StatementCategory))
        {
            var externalErrors = CheckIntegrationAsync(row).GetAwaiter().GetResult();
            errors.AddRange(externalErrors);
        }

        return errors;
    }

    private static bool ShouldCheckIntegration(StatementCategory category)
        => category is StatementCategory.CorpActionIncorporation
           or StatementCategory.CorpActionSplit
           or StatementCategory.CorpActionBonus
           or StatementCategory.CashIncomeDividend
           or StatementCategory.CashIncomeJCP;

    private static Task<IEnumerable<string>> CheckIntegrationAsync(B3StatementRow row)
    {
        // ponto futuro: integração com feed de eventos corporativos da B3
        return Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
    }

    // ----------------- helpers -----------------
    private static void Require(bool condition, List<string> errors, string message)
    {
        if (!condition) errors.Add(message);
    }

    private static void NotNegative(decimal? value, List<string> errors, string message)
    {
        if (value is not null && value < 0) errors.Add(message);
    }
}

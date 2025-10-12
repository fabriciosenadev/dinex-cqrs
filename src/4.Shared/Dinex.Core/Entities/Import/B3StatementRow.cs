using System.Globalization;

namespace Dinex.Core
{
    public class B3StatementRow : Entity
    {
        public Guid ImportJobId { get; private set; }
        public int RowNumber { get; private set; }

        // Podem ser nulos em linhas de erro
        public string? Asset { get; private set; }
        public OperationType? OperationType { get; private set; }

        // NOVO: evento do ativo (coluna "Movimentação")
        public string? Movement { get; private set; }

        public DateTime Date { get; private set; }
        public string? DueDate { get; private set; }
        public decimal? Quantity { get; private set; }
        public decimal? UnitPrice { get; private set; }
        public decimal? TotalValue { get; private set; }

        public string? Broker { get; private set; }
        public string RawLineJson { get; private set; } = null!;
        public B3StatementRowStatus Status { get; private set; }
        public string? Error { get; private set; }

        private B3StatementRow(
            Guid importJobId,
            int rowNumber,
            string? asset,
            OperationType? operationType,
            string? movement,
            DateTime date,
            string? dueDate,
            decimal? quantity,
            decimal? unitPrice,
            decimal? totalValue,
            string? broker,
            string rawLineJson,
            B3StatementRowStatus status,
            string? error,
            DateTime createdAt)
        {
            ImportJobId = importJobId;
            RowNumber = rowNumber;
            Asset = asset;
            OperationType = operationType;
            Movement = movement;
            Date = EnsureUtc(date);
            DueDate = dueDate;
            Quantity = quantity;
            UnitPrice = unitPrice;
            TotalValue = totalValue;
            Broker = broker;
            RawLineJson = rawLineJson;
            Status = status;
            Error = error;
            CreatedAt = createdAt;
        }

        private static DateTime EnsureUtc(DateTime date)
        {
            if (date == default)
                return DateTime.SpecifyKind(default, DateTimeKind.Utc);

            if (date.Kind == DateTimeKind.Utc)
                return date;

            if (date.Kind == DateTimeKind.Local)
                return date.ToUniversalTime();

            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }

        private static readonly HashSet<string> MovimentosSemPrecoObrigatorio = new(StringComparer.OrdinalIgnoreCase)
        {
            // --- Bonificações e Desdobramentos ---
            "Bonificação em Ativos",
            "Bonificação em Ações",
            "Desdobramento",
            "Grupamento",
            "Ajuste de Fração",
            "Ajuste de Frações de Ações",

            // --- Transferências e Movimentações Internas ---
            "Transferência – Liquidação",
            "Transferência – Liquidação de Ativos",
            "Transferência – Custódia",
            "Transferência de Custódia",
            "Transferência de Ativos entre Contas",
            "Transferência entre Contas Próprias",
            "Transferência para Terceiros",

            // --- Reorganizações Societárias ---
            "Incorporação",
            "Incorporação de Ações",
            "Cisão",
            "Cisão Parcial",
            "Fusão",
            "Reorganização Societária",
            "Conversão de Ações",
            "Conversão de Títulos em Ações",
            "Conversão de Debêntures em Ações",
            "Consolidação de Ações",
            "Reclassificação de Ações",

            // --- Subscrições e Direitos ---
            "Subscrição Gratuita",
            "Subscrição – Direito Não Exercido",
            "Subscrição – Conversão Automática",
            "Exercício de Direito de Subscrição (sem custo)",
            "Distribuição de Direitos",
            "Conversão de Direitos em Ações",

            // --- Ajustes e Regularizações ---
            "Reajuste de Posição",
            "Ajuste de Posição",
            "Ajuste Contábil",
            "Ajuste de Quantidade",
            "Regularização de Posição",
            "Retificação de Lançamento",
            "Correção de Evento Corporativo",

            // --- Integralizações e Aportes ---
            "Integralização de Capital",
            "Aporte de Capital",
            "Integralização de Ações",
            "Integralização de Quotas",

            // --- Outras Situações Especiais ---
            "Conversão de Moeda",
            "Conversão de Units",
            "Conversão de Classes de Ações",
            "Conversão de ADR em Ações",
            "Conversão de Ações em ADR",
            "Cancelamento de Units",
            "Cancelamento de Ações (sem custo)",
            "Distribuição de Ativos",
            "Distribuição de Bônus",
            "Distribuição de Ativos sem Contraprestação",
            "Evento de Reestruturação",
            "Evento sem Movimento Financeiro",
        };


        private static bool TryParseDatePtBr(string value, out DateTime date)
        {
            var styles = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;
            var formats = new[] { "dd/MM/yyyy", "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ssZ" };
            return DateTime.TryParseExact(value, formats, new CultureInfo("pt-BR"), styles, out date)
                   || DateTime.TryParse(value, new CultureInfo("pt-BR"), styles, out date);
        }

        public static B3StatementRow Create(
            Guid importJobId,
            int rowNumber,
            string asset,
            OperationType operationType,
            string movement,
            DateTime date,
            string? dueDate,
            decimal? quantity,
            decimal? unitPrice,
            decimal? totalValue,
            string broker,
            string rawLineJson)
        {
            var row = new B3StatementRow(
                importJobId,
                rowNumber,
                asset,
                operationType,
                movement,
                date,
                dueDate,
                quantity,
                unitPrice,
                totalValue,
                broker,
                rawLineJson,
                B3StatementRowStatus.Novo,
                error: null,
                createdAt: DateTime.UtcNow
            );

            var errors = new List<string>();

            // Ativo
            if (string.IsNullOrWhiteSpace(asset))
                errors.Add("Campo 'Ativo' é obrigatório e não foi informado.");

            // Movimento
            if (string.IsNullOrWhiteSpace(movement))
                errors.Add("Campo 'Movimentação' é obrigatório e não foi informado.");

            // Data
            if (row.Date == default)
                errors.Add("Campo 'Data' é obrigatório e está ausente ou inválido.");

            // Vencimento (opcional, mas se vier deve ser data válida)
            if (!string.IsNullOrWhiteSpace(dueDate) && !TryParseDatePtBr(dueDate!, out _))
                errors.Add("Campo 'Vencimento' foi informado, porém não é uma data válida.");

            // Quantidade
            if (quantity is null || quantity <= 0)
                errors.Add("Campo 'Quantidade' é obrigatório e deve ser maior que zero.");

            // Regras de preço/valor dependentes do movimento
            var exigePreco = !string.IsNullOrWhiteSpace(movement)
                             && !MovimentosSemPrecoObrigatorio.Contains(movement);

            if (exigePreco)
            {
                // OperationType faz sentido aqui (compra/venda, etc.)
                if (row.OperationType is null)
                    errors.Add("Campo 'Tipo de Operação' é obrigatório para esta movimentação.");

                if (unitPrice is null || unitPrice < 0)
                    errors.Add("Campo 'Preço Unitário' é obrigatório e não pode ser negativo.");

                if (totalValue is null || totalValue < 0)
                    errors.Add("Campo 'Valor Total' é obrigatório e não pode ser negativo.");

                // Conferência aritmética com tolerância
                if (quantity is not null && unitPrice is not null && totalValue is not null)
                {
                    var esperado = Math.Round(quantity.Value * unitPrice.Value, 2, MidpointRounding.AwayFromZero);
                    var diff = Math.Abs(esperado - totalValue.Value);
                    if (diff > 0.05m)
                        errors.Add("O 'Valor Total' não confere com 'Quantidade × Preço Unitário' (diferença acima da tolerância de R$ 0,05).");
                }
            }
            else
            {
                // Para movimentos sem preço obrigatório, se vierem valores negativos, ainda assim é erro
                if (unitPrice is not null && unitPrice < 0)
                    errors.Add("Campo 'Preço Unitário' não pode ser negativo.");
                if (totalValue is not null && totalValue < 0)
                    errors.Add("Campo 'Valor Total' não pode ser negativo.");
            }

            // Corretora
            if (string.IsNullOrWhiteSpace(broker))
                errors.Add("Campo 'Corretora' é obrigatório e não foi informado.");

            // Linha bruta
            if (string.IsNullOrWhiteSpace(rawLineJson))
                errors.Add("Campo 'RawLineJson' é obrigatório para rastreabilidade.");

            if (errors.Any())
            {
                row.Status = B3StatementRowStatus.Erro;
                row.Error = string.Join(" | ", errors);
            }
            else
            {
                row.Status = B3StatementRowStatus.Novo;
                row.Error = null;
            }

            return row;
        }

        public static B3StatementRow CreateError(Guid importJobId, int rowNumber, string rawLineJson, string errorMessage)
            => new B3StatementRow(
                importJobId,
                rowNumber,
                asset: null,
                operationType: null,
                movement: null,
                date: DateTime.SpecifyKind(default, DateTimeKind.Utc),
                dueDate: null,
                quantity: null,
                unitPrice: null,
                totalValue: null,
                broker: null,
                rawLineJson: rawLineJson,
                status: B3StatementRowStatus.Erro,
                error: string.IsNullOrWhiteSpace(errorMessage) ? "Erro não especificado durante a validação da linha." : errorMessage,
                createdAt: DateTime.UtcNow
            );
    }
}

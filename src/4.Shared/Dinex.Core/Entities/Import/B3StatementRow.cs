namespace Dinex.Core
{
    public class B3StatementRow : Entity
    {
        public Guid ImportJobId { get; private set; }
        public int RowNumber { get; private set; }

        public string? Asset { get; private set; }
        public OperationType? OperationType { get; private set; }
        public LedgerSide? LedgerSide { get; private set; }
        public StatementCategory StatementCategory { get; private set; }
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
        
        public bool? ProcessedTrade { get; private set; }
        public DateTime? ProcessedTradeAtUtc { get; private set; }


        private B3StatementRow(
            Guid importJobId,
            int rowNumber,
            string? asset,
            OperationType? operationType,
            LedgerSide? ledgerSide,
            StatementCategory statementCategory,
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
            LedgerSide = ledgerSide;
            StatementCategory = statementCategory;
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
            OperationType? operationType,
            LedgerSide? ledgerSide,
            StatementCategory statementCategory,
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
                ledgerSide,
                statementCategory,
                movement,
                date,
                dueDate,
                quantity,
                unitPrice,
                totalValue,
                broker,
                rawLineJson,
                B3StatementRowStatus.Novo,
                null,
                DateTime.UtcNow
            );

            // Validação estrutural básica
            var baseErrors = new List<string>();
            if (string.IsNullOrWhiteSpace(asset))
                baseErrors.Add("Campo 'Ativo' é obrigatório e não foi informado.");
            if (string.IsNullOrWhiteSpace(movement))
                baseErrors.Add("Campo 'Movimentação' é obrigatório e não foi informado.");
            if (row.Date == default)
                baseErrors.Add("Campo 'Data' é obrigatório e está ausente ou inválido.");
            if (string.IsNullOrWhiteSpace(broker))
                baseErrors.Add("Campo 'Corretora' é obrigatório e não foi informado.");
            if (string.IsNullOrWhiteSpace(rawLineJson))
                baseErrors.Add("Campo 'RawLineJson' é obrigatório para rastreabilidade.");
            if (!string.IsNullOrWhiteSpace(dueDate) && !TryParseDatePtBr(dueDate!, out _))
                baseErrors.Add("Campo 'Vencimento' foi informado, porém não é uma data válida.");

            // regras semânticas aplicadas externamente
            var semanticErrors = B3StatementRuleSet.Validate(row);

            var allErrors = baseErrors.Concat(semanticErrors).ToList();
            if (allErrors.Any())
            {
                row.Status = B3StatementRowStatus.Erro;
                row.Error = string.Join(" | ", allErrors);
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
                null,
                null,
                null,
                StatementCategory.Unknown,
                null,
                DateTime.SpecifyKind(default, DateTimeKind.Utc),
                null,
                null,
                null,
                null,
                null,
                rawLineJson,
                B3StatementRowStatus.Erro,
                string.IsNullOrWhiteSpace(errorMessage) ? "Erro não especificado durante a validação da linha." : errorMessage,
                DateTime.UtcNow
            );

        public void MarkTradeProcessed()
        {
            ProcessedTrade = true;
            ProcessedTradeAtUtc = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Status = B3StatementRowStatus.Processando;
        }

        public void MarkAsError(string message)
        {
            Status = B3StatementRowStatus.Erro;
            Error = string.IsNullOrWhiteSpace(message) ? "Erro ao processar trade." : message;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

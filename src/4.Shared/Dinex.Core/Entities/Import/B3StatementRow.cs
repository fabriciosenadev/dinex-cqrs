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

            if (string.IsNullOrWhiteSpace(asset))
                errors.Add("Campo Asset (Ativo) obrigatório e não informado.");

            if (string.IsNullOrWhiteSpace(movement))
                errors.Add("Campo Movement (evento do ativo) obrigatório e não informado.");

            if (row.Date == default)
                errors.Add("Campo Data obrigatório e não informado ou inválido.");

            if (!string.IsNullOrWhiteSpace(dueDate) && !DateTime.TryParse(dueDate, out _))
                errors.Add("Campo DueDate informado, porém não é uma data válida.");

            if (quantity is null || quantity <= 0)
                errors.Add("Campo Quantity obrigatório e deve ser maior que zero.");

            if (unitPrice is null || unitPrice < 0)
                errors.Add("Campo UnitPrice obrigatório e não pode ser negativo.");

            if (totalValue is null || totalValue < 0)
                errors.Add("Campo TotalValue obrigatório e não pode ser negativo.");

            if (quantity is not null && unitPrice is not null && totalValue is not null)
            {
                var expected = Math.Round(quantity.Value * unitPrice.Value, 2);
                if (Math.Abs(expected - totalValue.Value) > 0.03M)
                    errors.Add("Campo TotalValue não bate com Quantity x UnitPrice.");
            }

            if (string.IsNullOrWhiteSpace(broker))
                errors.Add("Campo Broker obrigatório e não informado.");

            if (string.IsNullOrWhiteSpace(rawLineJson))
                errors.Add("Campo RawLineJson obrigatório para rastreio.");

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
                error: errorMessage,
                createdAt: DateTime.UtcNow
            );
    }
}

namespace Dinex.Core;

public class InvestmentHistory : Entity
{
    public Guid QueueInId { get; private set; }
    public Applicable Applicable { get; private set; }
    public DateTime Date { get; private set; }
    public InvestmentTransactionType TransactionType { get; private set; }
    public string Product { get; private set; }
    public string Institution { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal OperationValue { get; private set; }

    private InvestmentHistory(
        Guid queueInId,
        Applicable applicable,
        DateTime date,
        InvestmentTransactionType transactionType,
        string product,
        string institution,
        int quantity,
        decimal unitPrice,
        decimal operationValue,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt)
    {
        QueueInId = queueInId;
        Applicable = applicable;
        Date = date;
        TransactionType = transactionType;
        Product = product;
        Institution = institution;
        Quantity = quantity;
        UnitPrice = unitPrice;
        OperationValue = operationValue;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DeletedAt = deletedAt;
    }

    public static InvestmentHistory Create(Dictionary<int, List<dynamic>> investingHistoryData, int selectedRow, Guid queueInId, CultureInfo culture)
    {
        var selectedApplicable = investingHistoryData.FirstOrDefault(x => x.Key == selectedRow).Value[0];
        var activityDate = investingHistoryData.FirstOrDefault(x => x.Key == selectedRow).Value[1];
        var selectedActivity = investingHistoryData.FirstOrDefault(x => x.Key == selectedRow).Value[2];
        var selectedProduct = investingHistoryData.FirstOrDefault(x => x.Key == selectedRow).Value[3];
        var selectedInstitution = investingHistoryData.FirstOrDefault(x => x.Key == selectedRow).Value[4];
        var selectedQuantity = investingHistoryData.FirstOrDefault(x => x.Key == selectedRow).Value[5];
        var selectedUnityPrice = investingHistoryData.FirstOrDefault(x => x.Key == selectedRow).Value[6];
        var selectedOperationValue = investingHistoryData.FirstOrDefault(x => x.Key == selectedRow).Value[7];

        var historyFile = new InvestmentHistory(
            queueInId,
            applicable: GetApplicable(selectedApplicable),
            date: DateTime.Parse(activityDate, culture),
            transactionType: GetInvestmentActivityTypeByDescription(selectedActivity),
            product: selectedProduct,
            institution: selectedInstitution,
            quantity: ConvertToInt(selectedQuantity),
            unitPrice: ConvertToDecimal(selectedUnityPrice),
            operationValue: ConvertToDecimal(selectedOperationValue),
            createdAt: DateTime.UtcNow,
            updatedAt: null,
            deletedAt: null);

        if (historyFile.TransactionType == InvestmentTransactionType.Unknown)
        {
            var errorMsg = $"Erro ao identificar tipo de atividade {selectedActivity}, para o produto {selectedProduct} na data {historyFile.Date}";
            historyFile.AddNotification("InvestingHistory.ActivityType", errorMsg);
        }

        return historyFile;
    }

    public static IEnumerable<InvestmentHistory> CreateRange(Dictionary<int, List<dynamic>> investingHistoryData, Guid queueInId, CultureInfo culture)
    {
        var historic = new List<InvestmentHistory>();
        for (int selectedRow = 0; selectedRow < investingHistoryData.Count; selectedRow++)
        {
            var investingHistory = Create(investingHistoryData, selectedRow, queueInId, culture);
            historic.Add(investingHistory);
        }
        return historic;
    }

    public void UpdateByProcessing()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    #region private methods
    private static InvestmentTransactionType GetInvestmentActivityTypeByDescription(string? description)
    {
        var enumValues = Enum.GetValues(typeof(InvestmentTransactionType));

        foreach (var enumValue in enumValues)
        {
            if (enumValue is InvestmentTransactionType activityType)
            {
                var enumDescription = GetEnumDescription(activityType);

                if (enumDescription == description)
                    return activityType;
            }
        }

        return InvestmentTransactionType.Unknown;
    }

    private static string GetEnumDescription(Enum enumValue)
    {
        var descriptionAttribute = enumValue.GetType()
            .GetField(enumValue.ToString())
            .GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;

        return descriptionAttribute?.Description ?? enumValue.ToString();
    }

    private static Applicable GetApplicable(string value)
    {
        if (value == "Credito")
        {
            return Applicable.In;
        }
        else
        {
            return Applicable.Out;
        }
    }

    private static int ConvertToInt(dynamic value)
    {
        var stringValue = Convert.ToString(value);

        var isInt = int.TryParse(stringValue, out int result);
        if (!isInt)
            return 0;

        return result;
    }

    private static decimal ConvertToDecimal(dynamic value)
    {
        var stringValue = Convert.ToString(value);

        var isDecimal = decimal.TryParse(stringValue, out decimal result);
        if (!isDecimal)
            return decimal.Zero;

        return result;
    }
    #endregion
}

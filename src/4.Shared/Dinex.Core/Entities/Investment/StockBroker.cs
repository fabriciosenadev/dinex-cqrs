namespace Dinex.Core;

public class StockBroker : Entity
{
    public string Name { get; private set; }

    private StockBroker(
        string name,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt)
    {

    }

    public static StockBroker Create(string stockBrokerName)
    {
        var newStockBroker = new StockBroker(
            name: stockBrokerName,
            createdAt: DateTime.UtcNow,
            updatedAt: null,
            deletedAt: null);

        return newStockBroker;
    }

    public static IEnumerable<StockBroker> CreateRange(IEnumerable<string> stockBrokerNames)
    {
        var stockBrokers = new List<StockBroker>();
        foreach (var stockBrokerName in stockBrokerNames)
        {
            var stockBroker = Create(stockBrokerName.Trim());
            stockBrokers.Add(stockBroker);
        }
        return stockBrokers;
    }
}

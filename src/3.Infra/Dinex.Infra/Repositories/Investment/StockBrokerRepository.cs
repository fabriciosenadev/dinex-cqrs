namespace Dinex.Infra;

public interface IStockBrokerRepository : IRepository<StockBroker>
{

}


public class StockBrokerRepository : Repository<StockBroker>, IStockBrokerRepository
{
    public StockBrokerRepository(DinexApiContext context) : base(context)
    {
    }
}

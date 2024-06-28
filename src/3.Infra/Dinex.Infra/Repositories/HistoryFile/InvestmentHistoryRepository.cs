namespace Dinex.Infra;

public interface IInvestmentHistoryRepository : IRepository<InvestmentHistory>
{

}

public class InvestmentHistoryRepository : Repository<InvestmentHistory>, IInvestmentHistoryRepository
{
    public InvestmentHistoryRepository(DinexApiContext context) : base(context)
    {
    }
}

namespace Dinex.Infra;

public interface IInvestmentTransactionRepository : IRepository<InvestmentTransaction> { }
public class InvestmentTransactionRepository : Repository<InvestmentTransaction>, IInvestmentTransactionRepository
{
    public InvestmentTransactionRepository(DinexApiContext context) : base(context)
    {
    }
}

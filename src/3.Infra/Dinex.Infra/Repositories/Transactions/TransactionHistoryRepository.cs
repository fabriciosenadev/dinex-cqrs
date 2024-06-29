namespace Dinex.Infra;

public interface ITransactionHistoryRepository : IRepository<TransactionHistory> { }

public class TransactionHistoryRepository : Repository<TransactionHistory>, ITransactionHistoryRepository
{
    public TransactionHistoryRepository(DinexApiContext context) : base(context)
    {
    }
}

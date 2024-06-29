namespace Dinex.Infra;

public interface IWalletRepository : IRepository<Wallet>
{

}

public class WalletRepository : Repository<Wallet>, IWalletRepository
{
    public WalletRepository(DinexApiContext context) : base(context)
    {
    }
}

namespace Dinex.Infra;

public class DinexApiContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<QueueIn> QueueIn { get; set; }

    #region investment
    public DbSet<InvestmentHistory> InvestmentHistory { get; set; }
    public DbSet<StockBroker> StockBrokers { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    #endregion

    #region transactions
    public DbSet<InvestmentTransaction> InvestmentTransactions { get; set; }
    public DbSet<TransactionHistory> TransactionHistories { get; set; }
    #endregion

    public DinexApiContext(DbContextOptions<DinexApiContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<Notifiable<Notification>>();
        modelBuilder.Ignore<Notification>();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DinexApiContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}

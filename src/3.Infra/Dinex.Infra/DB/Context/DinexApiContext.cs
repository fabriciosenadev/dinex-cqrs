namespace Dinex.Infra;

public class DinexApiContext : DbContext
{
    public DinexApiContext(DbContextOptions<DinexApiContext> options)
        : base(options) { }

    public DbSet<Operation> Assets => Set<Operation>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Broker> Brokers => Set<Broker>();
    public DbSet<Operation> Operations => Set<Operation>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();
    public DbSet<B3StatementRow> B3StatementRows => Set<B3StatementRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<Notifiable<Notification>>();
        modelBuilder.Ignore<Notification>();

        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DinexApiContext).Assembly);
    }
}

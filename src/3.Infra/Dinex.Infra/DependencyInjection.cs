namespace Dinex.Infra;

public static class DependencyInjection
{
    public static IServiceCollection RegisterInfraDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        //connection string
        services.AddDbContext<DinexApiContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DinexDB"))
        );

        #region generic repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        #endregion

        services.AddScoped<IUserRepository, UserRepository>();

        //services.AddScoped<IQueueInRepository, QueueInRepository>();

        #region investment
        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<IBrokerRepository, BrokerRepository>();
        services.AddScoped<IOperationRepository, OperationRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();

        //services.AddScoped<IStockBrokerRepository, StockBrokerRepository>();
        //services.AddScoped<IInvestmentHistoryRepository, InvestmentHistoryRepository>();
        //services.AddScoped<IInvestmentTransactionRepository, InvestmentTransactionRepository>();
        //services.AddScoped<ITransactionHistoryRepository, TransactionHistoryRepository>();
        #endregion

        return services;
    }
}

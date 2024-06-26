namespace Dinex.AppService;

public static class DependencyInjection
{
    public static IServiceCollection RegisterBusinessDependecies(this IServiceCollection services)
    {
        services.RegisterInfraDependencies();

        #region service
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ISendEmailService, SendEmailService>();
        #endregion

        return services;
    }
}

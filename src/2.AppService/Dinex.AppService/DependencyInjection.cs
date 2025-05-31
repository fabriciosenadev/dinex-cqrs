namespace Dinex.AppService;

public static class DependencyInjection
{
    public static IServiceCollection RegisterBusinessDependecies(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterInfraDependencies(configuration);

        #region service
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ISendEmailService, SendEmailService>();
        #endregion

        #region external service
        services.AddScoped<IJwtGenerator, JwtGenerator>();
        #endregion

        return services;
    }
}

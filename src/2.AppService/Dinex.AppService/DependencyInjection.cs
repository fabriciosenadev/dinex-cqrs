using Microsoft.Extensions.Configuration;

namespace Dinex.AppService;

public static class DependencyInjection
{
    public static IServiceCollection RegisterBusinessDependecies(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterInfraDependencies(configuration);

        #region service
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ISendEmailService, SendEmailService>();
        services.AddScoped<IQueueService, QueueService>();
        #endregion

        return services;
    }
}

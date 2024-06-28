using Microsoft.Extensions.Configuration;

namespace Dinex.Infra;

public static class DependencyInjection
{
    public static IServiceCollection RegisterInfraDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        //connection string
        services.AddDbContext<DinexApiContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DinexDB"))
        );

        services.AddTransient<IUserRepository, UserRepository>();
        services.AddScoped<IQueueInRepository, QueueInRepository>();

        return services;
    }
}

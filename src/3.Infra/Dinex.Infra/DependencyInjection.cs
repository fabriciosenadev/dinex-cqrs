namespace Dinex.Infra;

public static class DependencyInjection
{
    public static IServiceCollection RegisterInfraDependencies(this IServiceCollection services)
    {
        services.AddTransient<IUserRepository, UserRepository>();

        return services;
    }
}

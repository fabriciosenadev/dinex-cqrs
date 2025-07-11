using MediatorEasy.Extensions;

namespace Dinex.Api;

public static class DependecyInjectionConfig
{
    public static IServiceCollection RegisterAllDepdencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterBusinessDependecies(configuration);

        services.AddMediator();

        return services;
    }

    public static void AddMediator(this IServiceCollection services)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => assembly.GetTypes().Any(type =>
                type.IsPublic &&
                !type.IsAbstract &&
                type.IsClass &&
                (typeof(IQueryHandler).IsAssignableFrom(type) || typeof(ICommandHandler).IsAssignableFrom(type))))
            .ToArray();

        services.AddMediatorEasy(assemblies);
    }
}

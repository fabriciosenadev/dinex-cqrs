﻿namespace Dinex.AppService;

public static class DependencyInjection
{
    public static IServiceCollection RegisterBusinessDependecies(this IServiceCollection services)
    {
        services.RegisterInfraDependencies();

        return services;
    }
}

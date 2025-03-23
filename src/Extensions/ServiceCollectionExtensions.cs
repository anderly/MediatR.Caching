using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Caching.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediatRCaching(
        this IServiceCollection services)
    {
        services.AddSingleton<ICache, CacheService>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationAttributeBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingAttributeBehavior<,>));

        return services;
    }

    /// <summary>Adds all validators in specified assembly</summary>
    /// <param name="services">The collection of services</param>
    /// <param name="assembly">The assembly to scan</param>
    /// <param name="lifetime">The lifetime of the validators. The default is transient</param>
    /// <returns></returns>
    public static IServiceCollection AddCachePoliciesFromAssembly(
        this IServiceCollection services,
        Assembly assembly,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(ICachePolicy<,>)))
            //.AsSelf()
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(ICacheInvalidationPolicy<>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        return services;
    }
}
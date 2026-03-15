using Dotnet.Foundation.Abstractions.LifetimeServices;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using System.Reflection;

namespace Dotnet.Foundation.Extensions;

public static class LifetimeServiceExtensions
{
    private static readonly List<Type> LifetimeMarkers = [ typeof(IScopedService), typeof(ISingletonService), typeof(ITransientService) ];

    /// <summary>
    /// Registers as scoped service every implementation of <see cref = "IScopedService" /> with the matching interface (e.g. <c>SomeService</c> with <c>ISomeService</c>) to the service collection. <br />
    /// Registers as singleton service every implementation of <see cref = "ISingletonService" /> with the matching interface (e.g. <c>SomeService</c> with <c>ISomeService</c>) to the service collection. <br />
    /// Registers as transient service every implementation of <see cref = "ITransientService" /> with the matching interface (e.g. <c>SomeService</c> with <c>ISomeService</c>) to the service collection.
    /// </summary>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddLifetimeServices(this IServiceCollection serviceCollection, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);
        ArgumentNullException.ThrowIfNull(assembly);

        GuardAgainstServicesWithMultipleLifetimeMarkers(assembly);
        GuardAgainstServicesWithNoMatchingInterface(assembly);

        RegisterScopedServices(serviceCollection, assembly);
        RegisterSingletonServices(serviceCollection, assembly);
        RegisterTransientServices(serviceCollection, assembly);

        return serviceCollection;
    }

    private static void RegisterScopedServices(IServiceCollection serviceCollection, Assembly assembly)
    {
        serviceCollection.Scan(scan => scan
                                       .FromAssemblies(assembly)
                                       .AddClasses(filter => filter.AssignableTo<IScopedService>(), true)
                                       .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                                       .AsMatchingInterface()
                                       .WithScopedLifetime());
    }

    private static void RegisterSingletonServices(IServiceCollection serviceCollection, Assembly assembly)
    {
        serviceCollection.Scan(scan => scan
                                       .FromAssemblies(assembly)
                                       .AddClasses(filter => filter.AssignableTo<ISingletonService>(), true)
                                       .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                                       .AsMatchingInterface()
                                       .WithSingletonLifetime());
    }

    private static void RegisterTransientServices(IServiceCollection serviceCollection, Assembly assembly)
    {
        serviceCollection.Scan(scan => scan
                                       .FromAssemblies(assembly)
                                       .AddClasses(filter => filter.AssignableTo<ITransientService>(), true)
                                       .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                                       .AsMatchingInterface()
                                       .WithTransientLifetime());
    }

    private static void GuardAgainstServicesWithMultipleLifetimeMarkers(Assembly assembly)
    {
        var servicesWithMultipleLifetimeMarkers = assembly
                                                  .GetTypes()
                                                  .Where(type => type is { IsClass: true, IsAbstract: false })
                                                  .Where(type => LifetimeMarkers.Count(lifetimeMarker => lifetimeMarker.IsAssignableFrom(type)) > 1)
                                                  .ToList();

        if (servicesWithMultipleLifetimeMarkers.Any())
        {
            throw new InvalidOperationException(servicesWithMultipleLifetimeMarkers.Count == 1 ?
                $"Multiple lifetime marker implementations found for service '{servicesWithMultipleLifetimeMarkers.First().FullName}'." :
                $"Multiple lifetime marker implementations found for services {string.Join(", ", servicesWithMultipleLifetimeMarkers.Select(service => $"'{service.FullName}'"))}.");
        }
    }

    private static void GuardAgainstServicesWithNoMatchingInterface(Assembly assembly)
    {
        var servicesWithoutMatchingInterface = assembly
                                               .GetTypes()
                                               .Where(type => type is { IsClass: true, IsAbstract: false })
                                               .Where(type => LifetimeMarkers.Any(marker => marker.IsAssignableFrom(type)))
                                               .Where(type => !type.GetInterfaces().Any(i => i.Name == $"I{type.Name}"))
                                               .ToList();

        if (servicesWithoutMatchingInterface.Any())
        {
            throw new InvalidOperationException(servicesWithoutMatchingInterface.Count == 1 ?
                $"No matching interface found for service '{servicesWithoutMatchingInterface.First().FullName}'." :
                $"No matching interface found for services {string.Join(", ", servicesWithoutMatchingInterface.Select(service => $"'{service.FullName}'"))}.");
        }
    }
}

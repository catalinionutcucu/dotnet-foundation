using Dotnet.Foundation.Abstractions.LifetimeServices;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using System.Reflection;

namespace Dotnet.Foundation.Extensions;

/// <summary>
/// Provides extension members for registering the services marked with <see cref = "IScopedService" />, <see cref = "ISingletonService" /> or <see cref = "ITransientService" />.
/// </summary>
public static class LifetimeServiceExtensions
{
    private static readonly List<Type> LifetimeMarkers = [ typeof(IScopedService), typeof(ISingletonService), typeof(ITransientService) ];

    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers the services marked with a lifetime marker with the matching interface (e.g. <c>SomeService</c> with <c>ISomeService</c>) to the service collection:
        /// <list type = "bullet">
        /// <item><description>The implementations of <see cref = "IScopedService" /> as scoped services.</description></item>
        /// <item><description>The implementations of <see cref = "ISingletonService" /> as singleton services.</description></item>
        /// <item><description>The implementations of <see cref = "ITransientService" /> as transient services.</description></item>
        /// </list>
        /// </summary>
        /// <returns>The service collection.</returns>
        /// <exception cref = "InvalidOperationException">A service is marked with multiple lifetime markers or has no matching interface.</exception>
        public IServiceCollection AddLifetimeServices(Assembly assembly)
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
    }

    private static void RegisterScopedServices(IServiceCollection serviceCollection, Assembly assembly)
    {
        serviceCollection.Scan(scan => scan.FromAssemblies(assembly)
                                           .AddClasses(filter => filter.AssignableTo<IScopedService>(), false)
                                           .UsingRegistrationStrategy(RegistrationStrategy.Append)
                                           .AsMatchingInterface()
                                           .WithScopedLifetime());
    }

    private static void RegisterSingletonServices(IServiceCollection serviceCollection, Assembly assembly)
    {
        serviceCollection.Scan(scan => scan.FromAssemblies(assembly)
                                           .AddClasses(filter => filter.AssignableTo<ISingletonService>(), false)
                                           .UsingRegistrationStrategy(RegistrationStrategy.Append)
                                           .AsMatchingInterface()
                                           .WithSingletonLifetime());
    }

    private static void RegisterTransientServices(IServiceCollection serviceCollection, Assembly assembly)
    {
        serviceCollection.Scan(scan => scan.FromAssemblies(assembly)
                                           .AddClasses(filter => filter.AssignableTo<ITransientService>(), false)
                                           .UsingRegistrationStrategy(RegistrationStrategy.Append)
                                           .AsMatchingInterface()
                                           .WithTransientLifetime());
    }

    private static void GuardAgainstServicesWithMultipleLifetimeMarkers(Assembly assembly)
    {
        var assemblyTypes = assembly.GetTypes();

        var servicesWithMultipleLifetimeMarkers = assemblyTypes.Where(type => type is { IsClass: true, IsAbstract: false })
                                                               .Where(type => LifetimeMarkers.Count(lifetimeMarker => lifetimeMarker.IsAssignableFrom(type)) > 1)
                                                               .ToList();

        if (servicesWithMultipleLifetimeMarkers.Any())
        {
            throw new InvalidOperationException(servicesWithMultipleLifetimeMarkers.Count == 1 ?
                $"Multiple lifetime markers found for service type '{servicesWithMultipleLifetimeMarkers.First().FullName}'." :
                $"Multiple lifetime markers found for service types {string.Join(", ", servicesWithMultipleLifetimeMarkers.Select(service => $"'{service.FullName}'"))}.");
        }
    }

    private static void GuardAgainstServicesWithNoMatchingInterface(Assembly assembly)
    {
        var assemblyTypes = assembly.GetTypes();

        var servicesWithNoMatchingInterface = assemblyTypes.Where(type => type is { IsClass: true, IsAbstract: false })
                                                           .Where(type => LifetimeMarkers.Any(lifetimeMarker => lifetimeMarker.IsAssignableFrom(type)))
                                                           .Where(type => !type.GetInterfaces().Any(implementedInterface => implementedInterface.Name == $"I{type.Name}"))
                                                           .ToList();

        if (servicesWithNoMatchingInterface.Any())
        {
            throw new InvalidOperationException(servicesWithNoMatchingInterface.Count == 1 ?
                $"No matching interface found for service type '{servicesWithNoMatchingInterface.First().FullName}'." :
                $"No matching interface found for service types {string.Join(", ", servicesWithNoMatchingInterface.Select(service => $"'{service.FullName}'"))}.");
        }
    }
}

using Dotnet.Foundation.Abstractions.Endpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using System.Reflection;

namespace Dotnet.Foundation.Extensions;

public static class EndpointExtensions
{
    /// <summary>
    /// Registers the endpoints implementing <see cref = "IEndpoint" /> to the service collection.
    /// </summary>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddEndpoints(this IServiceCollection serviceCollection, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);
        ArgumentNullException.ThrowIfNull(assembly);

        serviceCollection.Scan(scan => scan.FromAssemblies(assembly)
                                           .AddClasses(filter => filter.AssignableTo<IEndpoint>(), true)
                                           .UsingRegistrationStrategy(RegistrationStrategy.Append)
                                           .As<IEndpoint>()
                                           .WithSingletonLifetime());

        return serviceCollection;
    }

    /// <summary>
    /// Maps the endpoints implementing <see cref = "IEndpoint" /> to the endpoint route builder.
    /// </summary>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        ArgumentNullException.ThrowIfNull(endpointRouteBuilder);

        var endpoints = endpointRouteBuilder.ServiceProvider.GetServices<IEndpoint>();

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(endpointRouteBuilder);
        }

        return endpointRouteBuilder;
    }
}

using Dotnet.Foundation.Abstractions.Requests;
using Dotnet.Foundation.Implementations;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Dotnet.Foundation.Extensions;

public static class RequestMediatorExtensions
{
    /// <summary>
    /// Registers the request mediator implementing <see cref = "IRequestMediator" /> to the service collection.
    /// </summary>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRequestMediator(this IServiceCollection serviceCollection, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);
        ArgumentNullException.ThrowIfNull(assembly);

        serviceCollection.AddScoped<IRequestMediator, RequestMediator>();

        return serviceCollection;
    }
}

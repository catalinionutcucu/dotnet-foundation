using Dotnet.Foundation.Abstractions.Requests;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using System.Reflection;

namespace Dotnet.Foundation.Extensions;

public static class RequestHandlerExtensions
{
    /// <summary>
    /// Registers the request handlers implementing <see cref = "IRequestHandler{TRequest,TResult}" /> or <see cref = "IRequestHandler{TRequest}" /> to the service collection.
    /// </summary>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRequestHandlers(this IServiceCollection serviceCollection, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);
        ArgumentNullException.ThrowIfNull(assembly);

        EnsureRequestsHaveMatchingRequestHandlers(assembly);

        serviceCollection.Scan(scan => scan.FromAssemblies(assembly)
                                           .AddClasses(filter => filter.AssignableTo(typeof(IRequestHandler<,>)))
                                           .UsingRegistrationStrategy(RegistrationStrategy.Throw)
                                           .As(type => type.GetInterfaces()
                                                           .Where(implementedInterface => AreTypesMatching(implementedInterface, typeof(IRequestHandler<,>), true))
                                                           .ToList())
                                           .WithScopedLifetime());

        serviceCollection.Scan(scan => scan.FromAssemblies(assembly)
                                           .AddClasses(filter => filter.AssignableTo(typeof(IRequestHandler<>)))
                                           .UsingRegistrationStrategy(RegistrationStrategy.Throw)
                                           .As(type => type.GetInterfaces()
                                                           .Where(implementedInterface => AreTypesMatching(implementedInterface, typeof(IRequestHandler<>), true))
                                                           .ToList())
                                           .WithScopedLifetime());

        return serviceCollection;
    }

    private static void EnsureRequestsHaveMatchingRequestHandlers(Assembly assembly)
    {
        var assemblyTypes = assembly.GetTypes();

        var requestTypes = new List<Type>();

        var requestTypesWithRequestHandler = new List<Type>();

        foreach (var type in assemblyTypes.Where(type => type is { IsAbstract: false, IsInterface: false }))
        {
            foreach (var implementedInterface in type.GetInterfaces())
            {
                if (AreTypesMatching(implementedInterface, typeof(IRequest<>), true) || AreTypesMatching(implementedInterface, typeof(IRequest), false))
                {
                    requestTypes.Add(type);
                }
                else if (AreTypesMatching(implementedInterface, typeof(IRequestHandler<,>), true) || AreTypesMatching(implementedInterface, typeof(IRequestHandler<>), true))
                {
                    requestTypesWithRequestHandler.Add(implementedInterface.GetGenericArguments()[0]);
                }
            }
        }

        var requestTypesWithNoRequestHandler = requestTypes.Where(requestType => !requestTypesWithRequestHandler.Any(requestType1 => requestType1 == requestType))
                                                           .ToList();

        var requestTypesWithMultipleRequestHandlers = requestTypes.Where(requestType => requestTypesWithRequestHandler.Count(requestType1 => requestType1 == requestType) > 1)
                                                                  .ToList();

        if (requestTypesWithNoRequestHandler.Any())
        {
            throw new InvalidOperationException(requestTypesWithNoRequestHandler.Count == 1 ?
                $"No request handler found for request type '{requestTypesWithNoRequestHandler.First().FullName}'." :
                $"No request handler found for request types {string.Join(", ", requestTypesWithNoRequestHandler.Select(requestType => $"'{requestType.FullName}'"))}.");
        }

        if (requestTypesWithMultipleRequestHandlers.Any())
        {
            throw new InvalidOperationException(requestTypesWithMultipleRequestHandlers.Count == 1 ?
                $"Multiple request handlers found for request type '{requestTypesWithMultipleRequestHandlers.First().FullName}'." :
                $"Multiple request handlers found for request types {string.Join(", ", requestTypesWithMultipleRequestHandlers.Select(requestType => $"'{requestType.FullName}'"))}.");
        }
    }

    private static bool AreTypesMatching(Type type1, Type type2, bool ignoreGenericTypeParameters = false)
    {
        if (ignoreGenericTypeParameters && type1.IsGenericType && type2.IsGenericType)
        {
            return type1.GetGenericTypeDefinition() == type2.GetGenericTypeDefinition();
        }

        return type1 == type2;
    }
}

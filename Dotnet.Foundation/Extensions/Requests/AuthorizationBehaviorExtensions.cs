using Dotnet.Foundation.Abstractions;
using Dotnet.Foundation.Abstractions.Requests;
using Dotnet.Foundation.Implementations.Requests;
using Dotnet.Foundation.Models;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using System.Reflection;

namespace Dotnet.Foundation.Extensions.Requests;

/// <summary>
/// Provides extension members for registering the request behavior checking the requests with the authorizers implementing <see cref = "IRequestAuthorizer{TRequest}" />.
/// </summary>
public static class AuthorizationBehaviorExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers the request behavior checking the requests and the authorizers implementing <see cref = "IRequestAuthorizer{TRequest}" /> to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        /// <exception cref = "InvalidOperationException">An authorized request has no result implementing <see cref = "IFailureResult{TResult,TError}" /> with an error of type <see cref = "RequestError" />.</exception>
        public IServiceCollection AddAuthorizationBehavior(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);
            ArgumentNullException.ThrowIfNull(assembly);

            GuardAgainstAuthorizedRequestsWithNoFailureResult(assembly);

            serviceCollection.AddRequestBehavior(typeof(AuthorizationBehavior<,>));

            serviceCollection.Scan(scan => scan.FromAssemblies(assembly)
                                               .AddClasses(filter => filter.AssignableTo(typeof(IRequestAuthorizer<>)), false)
                                               .UsingRegistrationStrategy(RegistrationStrategy.Append)
                                               .As(type => type.GetInterfaces()
                                                               .Where(implementedInterface => AreTypesMatching(implementedInterface, typeof(IRequestAuthorizer<>), true))
                                                               .ToList())
                                               .WithScopedLifetime());

            return serviceCollection;
        }
    }

    private static void GuardAgainstAuthorizedRequestsWithNoFailureResult(Assembly assembly)
    {
        var assemblyTypes = assembly.GetTypes();

        var authorizedRequestTypesWithNoFailureResult = assemblyTypes.Where(type => type is { IsClass: true, IsAbstract: false })
                                                                     .SelectMany(type => type.GetInterfaces())
                                                                     .Where(implementedInterface => AreTypesMatching(implementedInterface, typeof(IRequestAuthorizer<>), true))
                                                                     .Select(implementedInterface => implementedInterface.GetGenericArguments()[0])
                                                                     .Where(authorizedType => authorizedType.GetInterfaces().Any(implementedInterface => AreTypesMatching(implementedInterface, typeof(IRequest), false) || AreTypesMatching(implementedInterface, typeof(IRequest<>), true)))
                                                                     .Where(authorizedRequestType => !HasFailureResult(authorizedRequestType))
                                                                     .Distinct()
                                                                     .ToList();

        if (authorizedRequestTypesWithNoFailureResult.Any())
        {
            throw new InvalidOperationException(authorizedRequestTypesWithNoFailureResult.Count == 1 ?
                $"No failure result found for authorized request type '{authorizedRequestTypesWithNoFailureResult.First().FullName}'." :
                $"No failure result found for authorized request types {string.Join(", ", authorizedRequestTypesWithNoFailureResult.Select(authorizedRequestType => $"'{authorizedRequestType.FullName}'"))}.");
        }
    }

    private static bool HasFailureResult(Type requestType)
    {
        var resultType = requestType.GetInterfaces()
                                    .Where(implementedInterface => AreTypesMatching(implementedInterface, typeof(IRequest<>), true))
                                    .Select(implementedInterface => implementedInterface.GetGenericArguments()[0])
                                    .FirstOrDefault();

        return resultType is not null && typeof(IFailureResult<,>).MakeGenericType(resultType, typeof(RequestError)).IsAssignableFrom(resultType);
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

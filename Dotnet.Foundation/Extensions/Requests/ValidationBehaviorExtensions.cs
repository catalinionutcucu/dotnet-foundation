using Dotnet.Foundation.Abstractions;
using Dotnet.Foundation.Abstractions.Requests;
using Dotnet.Foundation.Implementations.Requests;
using Dotnet.Foundation.Models;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using System.Reflection;

namespace Dotnet.Foundation.Extensions.Requests;

/// <summary>
/// Provides extension members for registering the request behavior validating the requests with the validators implementing <see cref = "IValidator{T}" />.
/// </summary>
public static class ValidationBehaviorExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers the request behavior validating the requests and the validators implementing <see cref = "IValidator{T}" /> to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        /// <exception cref = "InvalidOperationException">A validated request has no result implementing <see cref = "IFailureResult{TResult,TError}" /> with an error of type <see cref = "RequestError" />.</exception>
        public IServiceCollection AddValidationBehavior(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);
            ArgumentNullException.ThrowIfNull(assembly);

            GuardAgainstValidatedRequestsWithNoFailureResult(assembly);

            serviceCollection.AddRequestBehavior(typeof(ValidationBehavior<,>));

            serviceCollection.Scan(scan => scan.FromAssemblies(assembly)
                                               .AddClasses(filter => filter.AssignableTo(typeof(IValidator<>)), false)
                                               .UsingRegistrationStrategy(RegistrationStrategy.Append)
                                               .As(type => type.GetInterfaces()
                                                               .Where(implementedInterface => AreTypesMatching(implementedInterface, typeof(IValidator<>), true))
                                                               .ToList())
                                               .WithScopedLifetime());

            return serviceCollection;
        }
    }

    private static void GuardAgainstValidatedRequestsWithNoFailureResult(Assembly assembly)
    {
        var assemblyTypes = assembly.GetTypes();

        var validatedRequestTypesWithNoFailureResult = assemblyTypes.Where(type => type is { IsClass: true, IsAbstract: false })
                                                                    .SelectMany(type => type.GetInterfaces())
                                                                    .Where(implementedInterface => AreTypesMatching(implementedInterface, typeof(IValidator<>), true))
                                                                    .Select(implementedInterface => implementedInterface.GetGenericArguments()[0])
                                                                    .Where(validatedType => validatedType.GetInterfaces().Any(implementedInterface => AreTypesMatching(implementedInterface, typeof(IRequest), false) || AreTypesMatching(implementedInterface, typeof(IRequest<>), true)))
                                                                    .Where(validatedRequestType => !HasFailureResult(validatedRequestType))
                                                                    .Distinct()
                                                                    .ToList();

        if (validatedRequestTypesWithNoFailureResult.Any())
        {
            throw new InvalidOperationException(validatedRequestTypesWithNoFailureResult.Count == 1 ?
                $"No failure result found for validated request type '{validatedRequestTypesWithNoFailureResult.First().FullName}'." :
                $"No failure result found for validated request types {string.Join(", ", validatedRequestTypesWithNoFailureResult.Select(validatedRequestType => $"'{validatedRequestType.FullName}'"))}.");
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

using Dotnet.Foundation.Abstractions.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnet.Foundation.Extensions.Requests;

/// <summary>
/// Provides extension members for registering the request behaviors implementing <see cref = "IRequestBehavior{TRequest,TResult}" /> or <see cref = "IRequestBehavior{TRequest}" />.
/// </summary>
public static class RequestBehaviorExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers the request behavior of the specified type implementing <see cref = "IRequestBehavior{TRequest,TResult}" /> or <see cref = "IRequestBehavior{TRequest}" /> to the service collection, running the request behaviors in the order of registration.
        /// </summary>
        /// <returns>The service collection.</returns>
        /// <exception cref = "InvalidOperationException">The type implements neither <see cref = "IRequestBehavior{TRequest,TResult}" /> nor <see cref = "IRequestBehavior{TRequest}" />.</exception>
        public IServiceCollection AddRequestBehavior(Type requestBehaviorType)
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);
            ArgumentNullException.ThrowIfNull(requestBehaviorType);

            var requestBehaviorInterfaces = requestBehaviorType.GetInterfaces()
                                                               .Where(implementedInterface => AreTypesMatching(implementedInterface, typeof(IRequestBehavior<,>), true) || AreTypesMatching(implementedInterface, typeof(IRequestBehavior<>), true))
                                                               .ToList();

            if (!requestBehaviorInterfaces.Any())
            {
                throw new InvalidOperationException($"No request behavior interface found for type '{requestBehaviorType.FullName}'.");
            }

            foreach (var requestBehaviorInterface in requestBehaviorInterfaces)
            {
                serviceCollection.AddScoped(requestBehaviorType.IsGenericTypeDefinition ? requestBehaviorInterface.GetGenericTypeDefinition() : requestBehaviorInterface, requestBehaviorType);
            }

            return serviceCollection;
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

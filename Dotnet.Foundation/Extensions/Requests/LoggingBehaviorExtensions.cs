using Dotnet.Foundation.Implementations.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnet.Foundation.Extensions.Requests;

/// <summary>
/// Provides extension members for registering the request behaviors logging the duration of the requests with their failure results and exceptions.
/// </summary>
public static class LoggingBehaviorExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers the request behaviors logging the duration of the requests with their failure results and exceptions to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddLoggingBehavior()
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);

            serviceCollection.AddSingleton(TimeProvider.System);

            serviceCollection.AddRequestBehavior(typeof(LoggingBehavior<,>));

            serviceCollection.AddRequestBehavior(typeof(LoggingBehavior<>));

            return serviceCollection;
        }
    }
}

using Dotnet.Foundation.Abstractions.Requests;
using Dotnet.Foundation.Implementations.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnet.Foundation.Extensions.Requests;

/// <summary>
/// Provides extension members for registering the request behavior caching the results of the requests implementing <see cref = "ICacheableRequest" />.
/// </summary>
public static class CachingBehaviorExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers the request behavior caching the results of the requests implementing <see cref = "ICacheableRequest" />, and the cache it uses, to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddCachingBehavior()
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);

            serviceCollection.AddCachingHandler();

            serviceCollection.AddRequestBehavior(typeof(CachingBehavior<,>));

            return serviceCollection;
        }
    }
}

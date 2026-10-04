using Dotnet.Foundation.Abstractions;
using Dotnet.Foundation.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnet.Foundation.Extensions;

/// <summary>
/// Provides extension members for registering the cache implementing <see cref = "ICachingHandler" />.
/// </summary>
public static class CachingHandlerExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers the cache implementing <see cref = "ICachingHandler" /> to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddCachingHandler()
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);

            serviceCollection.AddDistributedMemoryCache();

            serviceCollection.AddSingleton<ICachingHandler, CachingHandler>();

            return serviceCollection;
        }
    }
}

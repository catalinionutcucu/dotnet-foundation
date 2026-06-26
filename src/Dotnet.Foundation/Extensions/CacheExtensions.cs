using Dotnet.Foundation.Abstractions.Caching;
using Dotnet.Foundation.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dotnet.Foundation.Extensions;

public static class CacheExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers the cache implementing <see cref = "ICache" /> to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddCache()
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);

            serviceCollection.AddDistributedMemoryCache();

            serviceCollection.TryAddSingleton<ICache, Cache>();

            return serviceCollection;
        }
    }
}

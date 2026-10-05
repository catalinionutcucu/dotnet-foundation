using Dotnet.Foundation.Implementations.Requests;
using Dotnet.Foundation.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnet.Foundation.Extensions.Requests;

/// <summary>
/// Provides extension members for registering the request behavior returning a failure result with a <see cref = "RequestError" /> of type <see cref = "RequestErrorType.ResourceConflict" /> on concurrency conflicts.
/// </summary>
public static class ConcurrencyBehaviorExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers the request behavior returning a failure result with a <see cref = "RequestError" /> of type <see cref = "RequestErrorType.ResourceConflict" /> on concurrency conflicts to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddConcurrencyBehavior()
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);

            serviceCollection.AddRequestBehavior(typeof(ConcurrencyBehavior<,>));

            return serviceCollection;
        }
    }
}

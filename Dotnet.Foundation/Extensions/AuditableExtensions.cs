using Dotnet.Foundation.Abstractions;
using Dotnet.Foundation.Implementations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnet.Foundation.Extensions;

/// <summary>
/// Provides extension members for registering the interceptor for the entities implementing <see cref = "IAuditable" />.
/// </summary>
public static class AuditableExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers the interceptor setting the creation and update timestamps of the entities implementing <see cref = "IAuditable" /> to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddAuditableInterceptor()
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);

            serviceCollection.AddSingleton(TimeProvider.System);

            serviceCollection.AddSingleton<ISaveChangesInterceptor, AuditableInterceptor>();

            return serviceCollection;
        }
    }
}

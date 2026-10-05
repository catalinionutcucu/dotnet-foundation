using Dotnet.Foundation.Abstractions.Auditable;
using Dotnet.Foundation.Implementations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnet.Foundation.Extensions;

/// <summary>
/// Provides extension members for registering the interceptor for the entities implementing <see cref = "IAuditable" /> or <see cref = "IUserAuditable" />.
/// </summary>
public static class AuditableExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers the interceptor setting the creation and update timestamps of the entities implementing <see cref = "IAuditable" />, and the users creating and updating the entities implementing <see cref = "IUserAuditable" />, to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddAuditableInterceptor()
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);

            serviceCollection.AddSingleton(TimeProvider.System);

            serviceCollection.AddCurrentUser();

            serviceCollection.AddSingleton<ISaveChangesInterceptor, AuditableInterceptor>();

            return serviceCollection;
        }
    }
}

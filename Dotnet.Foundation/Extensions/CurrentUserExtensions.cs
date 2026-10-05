using Dotnet.Foundation.Abstractions;
using Dotnet.Foundation.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnet.Foundation.Extensions;

/// <summary>
/// Provides extension members for registering the current user implementing <see cref = "ICurrentUser" />.
/// </summary>
public static class CurrentUserExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers the current user implementing <see cref = "ICurrentUser" /> to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddCurrentUser()
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);

            serviceCollection.AddHttpContextAccessor();

            serviceCollection.AddSingleton<ICurrentUser, CurrentUser>();

            return serviceCollection;
        }
    }
}

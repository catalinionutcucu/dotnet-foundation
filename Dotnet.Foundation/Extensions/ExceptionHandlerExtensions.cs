using Dotnet.Foundation.Implementations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnet.Foundation.Extensions;

/// <summary>
/// Provides extension members for registering the exception handler implementing <see cref = "IExceptionHandler" />.
/// </summary>
public static class ExceptionHandlerExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers the exception handler implementing <see cref = "IExceptionHandler" /> to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddExceptionHandler()
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);

            serviceCollection.AddProblemDetails();

            serviceCollection.AddExceptionHandler<ExceptionHandler>();

            return serviceCollection;
        }
    }
}

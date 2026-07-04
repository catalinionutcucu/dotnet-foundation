using Dotnet.Foundation.Abstractions.Requests;
using Dotnet.Foundation.Implementations;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Dotnet.Foundation.Extensions;

public static class RequestMediatorExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers the request mediator implementing <see cref = "IRequestMediator" /> to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddRequestMediator(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);
            ArgumentNullException.ThrowIfNull(assembly);

            serviceCollection.AddScoped<IRequestMediator, RequestMediator>();

            return serviceCollection;
        }
    }
}

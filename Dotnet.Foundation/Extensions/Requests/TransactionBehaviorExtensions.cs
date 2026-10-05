using Dotnet.Foundation.Abstractions.Requests;
using Dotnet.Foundation.Implementations.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnet.Foundation.Extensions.Requests;

/// <summary>
/// Provides extension members for registering the request behaviors handling the requests implementing <see cref = "ITransactionalRequest" /> in a database transaction.
/// </summary>
public static class TransactionBehaviorExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers the request behaviors handling the requests implementing <see cref = "ITransactionalRequest" /> in a transaction of the database context of type <typeparamref name = "TDbContext" /> to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddTransactionBehavior<TDbContext>()
            where TDbContext : DbContext
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);

            serviceCollection.AddScoped<DbContext>(serviceProvider => serviceProvider.GetRequiredService<TDbContext>());

            serviceCollection.AddRequestBehavior(typeof(TransactionBehavior<,>));

            serviceCollection.AddRequestBehavior(typeof(TransactionBehavior<>));

            return serviceCollection;
        }
    }
}

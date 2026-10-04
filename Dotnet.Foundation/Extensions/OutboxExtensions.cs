using Dotnet.Foundation.Implementations.Outbox;
using Dotnet.Foundation.Models.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnet.Foundation.Extensions;

/// <summary>
/// Provides extension members for registering the outbox and applying the configuration for the outbox messages of type <see cref = "OutboxMessage" />.
/// </summary>
public static class OutboxExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers the interceptor storing the domain events as outbox messages and the background service publishing the outbox messages stored in the database context of type <typeparamref name = "TDbContext" /> to the service collection, validating the outbox options on start.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddOutbox<TDbContext>(Action<OutboxOptions>? configureOptions = null)
            where TDbContext : DbContext
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);

            serviceCollection.AddOptions<OutboxOptions>()
                             .Configure(outboxOptions => configureOptions?.Invoke(outboxOptions))
                             .Validate(outboxOptions => outboxOptions.Interval > TimeSpan.Zero && outboxOptions.BatchSize > 0 && outboxOptions.MaxAttempts > 0, "The interval, batch size and max attempts of the outbox options must be positive.")
                             .Validate(outboxOptions => outboxOptions.RetryDelay > TimeSpan.Zero && outboxOptions.MaxRetryDelay >= outboxOptions.RetryDelay, "The retry delay of the outbox options must be positive and not greater than the max retry delay.")
                             .Validate(outboxOptions => outboxOptions.ClaimDuration > TimeSpan.Zero && outboxOptions.RetentionPeriod > TimeSpan.Zero, "The claim duration and retention period of the outbox options must be positive.")
                             .ValidateOnStart();

            serviceCollection.AddSingleton(TimeProvider.System);

            serviceCollection.AddSingleton<ISaveChangesInterceptor, OutboxInterceptor>();

            serviceCollection.AddHostedService<OutboxProcessor<TDbContext>>();

            return serviceCollection;
        }
    }

    extension(ModelBuilder modelBuilder)
    {
        /// <summary>
        /// Applies the configuration for the outbox messages of type <see cref = "OutboxMessage" /> to the model builder.
        /// </summary>
        /// <returns>The model builder.</returns>
        public ModelBuilder ApplyOutboxMessageConfiguration()
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);

            modelBuilder.Entity<OutboxMessage>(entityTypeBuilder =>
            {
                entityTypeBuilder.HasKey(outboxMessage => outboxMessage.Id);

                entityTypeBuilder.Property(outboxMessage => outboxMessage.Version)
                                 .IsConcurrencyToken();

                entityTypeBuilder.PrimitiveCollection(outboxMessage => outboxMessage.CompletedHandlers);

                entityTypeBuilder.HasIndex(outboxMessage => new { outboxMessage.ProcessedAt, outboxMessage.OccurredAt });
            });

            return modelBuilder;
        }
    }
}

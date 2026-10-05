using Dotnet.Foundation.Abstractions;
using Dotnet.Foundation.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnet.Foundation.Extensions;

/// <summary>
/// Provides extension members for registering the interceptor, applying the concurrency tokens and expecting the versions for the entities implementing <see cref = "IVersionable" />.
/// </summary>
public static class VersionableExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers the interceptor changing the version of the added and modified entities implementing <see cref = "IVersionable" /> to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddVersionableInterceptor()
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);

            serviceCollection.AddSingleton<ISaveChangesInterceptor, VersionableInterceptor>();

            return serviceCollection;
        }
    }

    extension(ModelBuilder modelBuilder)
    {
        /// <summary>
        /// Applies the version as concurrency token to the entities implementing <see cref = "IVersionable" /> in the model builder.
        /// </summary>
        /// <returns>The model builder.</returns>
        public ModelBuilder ApplyVersionableConcurrencyTokens()
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);

            var versionableEntityTypes = modelBuilder.Model.GetEntityTypes()
                                                     .Where(entityType => entityType.BaseType is null && !entityType.IsOwned())
                                                     .Where(entityType => typeof(IVersionable).IsAssignableFrom(entityType.ClrType))
                                                     .ToList();

            foreach (var versionableEntityType in versionableEntityTypes)
            {
                modelBuilder.Entity(versionableEntityType.ClrType)
                            .Property(nameof(IVersionable.Version))
                            .IsConcurrencyToken();
            }

            return modelBuilder;
        }
    }

    extension<TEntity>(EntityEntry<TEntity> entityEntry)
        where TEntity : class, IVersionable
    {
        /// <summary>
        /// Sets the version the entity is expected to have when saving changes, so saving fails with a concurrency conflict if the entity was changed since that version.
        /// </summary>
        /// <returns>The entity entry.</returns>
        public EntityEntry<TEntity> ExpectVersion(Guid version)
        {
            ArgumentNullException.ThrowIfNull(entityEntry);

            entityEntry.Property(entity => entity.Version).OriginalValue = version;

            return entityEntry;
        }
    }
}

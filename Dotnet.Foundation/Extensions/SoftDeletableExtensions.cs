using Dotnet.Foundation.Abstractions.SoftDeletable;
using Dotnet.Foundation.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace Dotnet.Foundation.Extensions;

/// <summary>
/// Provides extension members for registering the interceptor and applying the query filter for the entities implementing <see cref = "ISoftDeletable" /> or <see cref = "IUserSoftDeletable" />.
/// </summary>
public static class SoftDeletableExtensions
{
    public const string QueryFilterName = "SoftDeletable";

    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers the interceptor setting the deletion timestamp of the entities implementing <see cref = "ISoftDeletable" />, and the user deleting the entities implementing <see cref = "IUserSoftDeletable" />, instead of removing them to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddSoftDeletableInterceptor()
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);

            serviceCollection.AddSingleton(TimeProvider.System);

            serviceCollection.AddCurrentUser();

            serviceCollection.AddSingleton<ISaveChangesInterceptor, SoftDeletableInterceptor>();

            return serviceCollection;
        }
    }

    extension(ModelBuilder modelBuilder)
    {
        /// <summary>
        /// Applies the query filter named <see cref = "QueryFilterName" /> excluding the deleted entities to the entities implementing <see cref = "ISoftDeletable" /> in the model builder.
        /// </summary>
        /// <returns>The model builder.</returns>
        public ModelBuilder ApplySoftDeletableQueryFilters()
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);

            var softDeletableEntityTypes = modelBuilder.Model.GetEntityTypes()
                                                       .Where(entityType => entityType.BaseType is null && !entityType.IsOwned())
                                                       .Where(entityType => typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                                                       .ToList();

            foreach (var softDeletableEntityType in softDeletableEntityTypes)
            {
                var entity = Expression.Parameter(softDeletableEntityType.ClrType, "entity");

                var queryFilter = Expression.Lambda(Expression.Equal(Expression.Property(entity, nameof(ISoftDeletable.DeletedAt)), Expression.Constant(null, typeof(DateTimeOffset?))), entity);

                modelBuilder.Entity(softDeletableEntityType.ClrType)
                            .HasQueryFilter(QueryFilterName, queryFilter);
            }

            return modelBuilder;
        }
    }
}

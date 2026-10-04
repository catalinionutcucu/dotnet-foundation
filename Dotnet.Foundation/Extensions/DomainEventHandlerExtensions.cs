using Dotnet.Foundation.Abstractions.DomainEvents;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using System.Reflection;

namespace Dotnet.Foundation.Extensions;

/// <summary>
/// Provides extension members for registering the domain event handlers implementing <see cref = "IDomainEventHandler{TDomainEvent}" />.
/// </summary>
public static class DomainEventHandlerExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        /// Registers the domain event handlers implementing <see cref = "IDomainEventHandler{TDomainEvent}" /> to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddDomainEventHandlers(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);
            ArgumentNullException.ThrowIfNull(assembly);

            serviceCollection.Scan(scan => scan.FromAssemblies(assembly)
                                               .AddClasses(filter => filter.AssignableTo(typeof(IDomainEventHandler<>)), false)
                                               .UsingRegistrationStrategy(RegistrationStrategy.Append)
                                               .As(type => type.GetInterfaces()
                                                               .Where(implementedInterface => AreTypesMatching(implementedInterface, typeof(IDomainEventHandler<>), true))
                                                               .ToList())
                                               .WithScopedLifetime());

            return serviceCollection;
        }
    }

    private static bool AreTypesMatching(Type type1, Type type2, bool ignoreGenericTypeParameters = false)
    {
        if (ignoreGenericTypeParameters && type1.IsGenericType && type2.IsGenericType)
        {
            return type1.GetGenericTypeDefinition() == type2.GetGenericTypeDefinition();
        }

        return type1 == type2;
    }
}

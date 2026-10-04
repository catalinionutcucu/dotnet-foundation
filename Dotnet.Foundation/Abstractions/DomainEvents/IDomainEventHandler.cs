namespace Dotnet.Foundation.Abstractions.DomainEvents;

/// <summary>
/// Defines a handler for domain events of type <typeparamref name = "TDomainEvent" />.
/// </summary>
public interface IDomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    /// <summary>
    /// Asynchronously handles a domain event of type <typeparamref name = "TDomainEvent" />.
    /// </summary>
    public Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
}

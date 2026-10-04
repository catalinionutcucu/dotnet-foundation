namespace Dotnet.Foundation.Abstractions.DomainEvents;

/// <summary>
/// Marks a domain event raised by an entity and handled by the corresponding handlers of type <see cref = "IDomainEventHandler{TDomainEvent}" />.
/// </summary>
public interface IDomainEvent;

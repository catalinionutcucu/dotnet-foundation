using Dotnet.Foundation.Abstractions.DomainEvents;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dotnet.Foundation.Primitives;

/// <summary>
/// Represents an entity identified by a <see cref = "Guid" />, compared by its type and identifier and raising domain events of type <see cref = "IDomainEvent" />.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    [NotMapped]
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    private readonly List<IDomainEvent> _domainEvents = [ ];

    /// <summary>
    /// Clears the domain events raised by the entity.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <inheritdoc />
    public bool Equals(Entity? other)
    {
        return other is not null && GetType() == other.GetType() && Id == other.Id;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Entity other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !Equals(left, right);
    }

    /// <summary>
    /// Raises a domain event of type <see cref = "IDomainEvent" /> from the entity.
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        _domainEvents.Add(domainEvent);
    }
}

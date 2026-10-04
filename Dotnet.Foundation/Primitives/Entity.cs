namespace Dotnet.Foundation.Primitives;

/// <summary>
/// Represents an entity identified by a <see cref = "Guid" /> and compared by its type and identifier.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

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
}

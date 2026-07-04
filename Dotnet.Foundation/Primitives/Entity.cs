namespace Dotnet.Foundation.Primitives;

public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public bool Equals(Entity? other)
    {
        return other is not null && GetType() == other.GetType() && Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity other && Equals(other);
    }

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

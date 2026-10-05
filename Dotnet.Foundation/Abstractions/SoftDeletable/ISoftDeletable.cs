namespace Dotnet.Foundation.Abstractions.SoftDeletable;

/// <summary>
/// Defines an entity with a deletion timestamp set instead of being removed when saving changes.
/// </summary>
public interface ISoftDeletable
{
    public DateTimeOffset? DeletedAt { get; }
}

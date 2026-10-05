namespace Dotnet.Foundation.Abstractions.SoftDeletable;

/// <summary>
/// Defines an entity with a deletion timestamp and the user deleting it set instead of being removed when saving changes.
/// </summary>
public interface IUserSoftDeletable : ISoftDeletable
{
    public string? DeletedBy { get; }
}

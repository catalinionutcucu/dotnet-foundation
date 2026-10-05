namespace Dotnet.Foundation.Abstractions;

/// <summary>
/// Defines an entity with a version changed when saving changes, so changes made to the entity since a known version are detected as concurrency conflicts.
/// </summary>
public interface IVersionable
{
    public Guid Version { get; }
}

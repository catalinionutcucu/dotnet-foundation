namespace Dotnet.Foundation.Abstractions;

/// <summary>
/// Defines an entity with creation and update timestamps set when saving changes.
/// </summary>
public interface IAuditable
{
    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? UpdatedAt { get; }
}

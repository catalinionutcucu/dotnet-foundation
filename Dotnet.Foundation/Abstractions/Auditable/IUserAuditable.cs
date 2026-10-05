namespace Dotnet.Foundation.Abstractions.Auditable;

/// <summary>
/// Defines an entity with creation and update timestamps and the users creating and updating it set when saving changes.
/// </summary>
public interface IUserAuditable : IAuditable
{
    public string? CreatedBy { get; }

    public string? UpdatedBy { get; }
}

namespace Dotnet.Foundation.Abstractions;

/// <summary>
/// Defines the user of the current request, with no identifier when the user is not authenticated or there is no current request.
/// </summary>
public interface ICurrentUser
{
    public string? Id { get; }
}

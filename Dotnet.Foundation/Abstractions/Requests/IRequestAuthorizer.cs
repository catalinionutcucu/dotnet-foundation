namespace Dotnet.Foundation.Abstractions.Requests;

/// <summary>
/// Defines an authorizer checking whether the current user is allowed to perform requests of type <typeparamref name = "TRequest" />.
/// </summary>
public interface IRequestAuthorizer<TRequest>
{
    /// <summary>
    /// Asynchronously checks whether the current user is allowed to perform a request of type <typeparamref name = "TRequest" />.
    /// </summary>
    /// <returns><see langword = "true" /> if the current user is allowed to perform the request; otherwise, <see langword = "false" />.</returns>
    public Task<bool> IsAuthorizedAsync(TRequest request, CancellationToken cancellationToken = default);
}

using Dotnet.Foundation.Abstractions;
using Dotnet.Foundation.Abstractions.Requests;
using Dotnet.Foundation.Models;

namespace Dotnet.Foundation.Implementations.Requests;

/// <summary>
/// Represents the request behavior implementing <see cref = "IRequestBehavior{TRequest,TResult}" />, checking the requests with the authorizers implementing <see cref = "IRequestAuthorizer{TRequest}" /> and returning a failure result with a <see cref = "RequestError" /> of type <see cref = "RequestErrorType.RequestNotAllowed" /> instead of calling the next behavior or the handler when an authorizer does not allow the request.
/// </summary>
public sealed class AuthorizationBehavior<TRequest, TResult> : IRequestBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : IFailureResult<TResult, RequestError>
{
    private readonly IEnumerable<IRequestAuthorizer<TRequest>> _authorizers;

    public AuthorizationBehavior(IEnumerable<IRequestAuthorizer<TRequest>> authorizers)
    {
        _authorizers = authorizers;
    }

    /// <inheritdoc />
    public async Task<TResult> HandleAsync(TRequest request, Func<Task<TResult>> next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        foreach (var authorizer in _authorizers)
        {
            if (!await authorizer.IsAuthorizedAsync(request, cancellationToken).ConfigureAwait(false))
            {
                return TResult.Failure(RequestError.RequestNotAllowed("request.not_allowed", "The current user is not allowed to perform the request."));
            }
        }

        return await next().ConfigureAwait(false);
    }
}

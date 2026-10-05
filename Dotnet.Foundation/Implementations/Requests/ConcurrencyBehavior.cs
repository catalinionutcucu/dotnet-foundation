using Dotnet.Foundation.Abstractions;
using Dotnet.Foundation.Abstractions.Requests;
using Dotnet.Foundation.Models;
using Microsoft.EntityFrameworkCore;

namespace Dotnet.Foundation.Implementations.Requests;

/// <summary>
/// Represents the request behavior implementing <see cref = "IRequestBehavior{TRequest,TResult}" />, returning a failure result with a <see cref = "RequestError" /> of type <see cref = "RequestErrorType.ResourceConflict" /> when the next behavior or the handler fails with a concurrency conflict.
/// </summary>
public sealed class ConcurrencyBehavior<TRequest, TResult> : IRequestBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : IFailureResult<TResult, RequestError>
{
    /// <inheritdoc />
    public async Task<TResult> HandleAsync(TRequest request, Func<Task<TResult>> next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        try
        {
            return await next().ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException)
        {
            return TResult.Failure(RequestError.ResourceConflict("resource.conflict", "The resource was changed by another request."));
        }
    }
}

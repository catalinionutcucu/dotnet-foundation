using Dotnet.Foundation.Abstractions;
using Dotnet.Foundation.Abstractions.Requests;
using Dotnet.Foundation.Models;

namespace Dotnet.Foundation.Implementations.Requests;

/// <summary>
/// Represents the request behavior implementing <see cref = "IRequestBehavior{TRequest,TResult}" />, returning the cached result of the requests implementing <see cref = "ICacheableRequest" /> instead of calling the next behavior or the handler, and caching the results that are neither <see langword = "null" /> nor failure results.
/// </summary>
public sealed class CachingBehavior<TRequest, TResult> : IRequestBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>, ICacheableRequest
{
    private readonly ICachingHandler _cachingHandler;

    public CachingBehavior(ICachingHandler cachingHandler)
    {
        _cachingHandler = cachingHandler;
    }

    /// <inheritdoc />
    public async Task<TResult> HandleAsync(TRequest request, Func<Task<TResult>> next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        var cachedResult = await _cachingHandler.GetAsync<CachedResult>(request.CacheKey, cancellationToken)
                                                .ConfigureAwait(false);

        if (cachedResult is not null)
        {
            return cachedResult.Result;
        }

        var result = await next().ConfigureAwait(false);

        if (result is not null && result is not IFailureResult<TResult, RequestError> { IsFailure: true })
        {
            await _cachingHandler.SetAsync(request.CacheKey, new CachedResult(result), request.CacheExpiration, cancellationToken)
                                 .ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Represents a cached result, telling a cached default value apart from a missing one.
    /// </summary>
    private sealed record CachedResult(TResult Result);
}

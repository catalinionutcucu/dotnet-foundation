namespace Dotnet.Foundation.Abstractions.Requests;

/// <summary>
/// Defines a request with its result cached under the cache key, expiring after the cache expiration if one is provided.
/// </summary>
public interface ICacheableRequest
{
    public string CacheKey { get; }

    public TimeSpan? CacheExpiration { get; }
}

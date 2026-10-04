namespace Dotnet.Foundation.Abstractions;

/// <summary>
/// Defines a cache for storing and retrieving values under string keys.
/// </summary>
public interface ICachingHandler
{
    /// <summary>
    /// Asynchronously gets the value under the specified key from the cache.
    /// </summary>
    /// <returns>The value under the specified key from the cache, or the default value of <typeparamref name = "TValue" /> if the key is not found.</returns>
    public Task<TValue?> GetAsync<TValue>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously sets the value under the specified key in the cache, expiring after the specified expiration if one is provided.
    /// </summary>
    public Task SetAsync<TValue>(string key, TValue value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously removes the value under the specified key from the cache.
    /// </summary>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

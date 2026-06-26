namespace Dotnet.Foundation.Abstractions.Caching;

public interface ICache
{
    /// <summary>
    /// Gets asynchronously the value under the specified key from the cache.
    /// </summary>
    /// <returns>The value under the specified key from the cache.</returns>
    public Task<TValue?> GetAsync<TValue>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets asynchronously the value under the specified key in the cache.
    /// </summary>
    public Task SetAsync<TValue>(string key, TValue value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes asynchronously the value under the specified key from the cache.
    /// </summary>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

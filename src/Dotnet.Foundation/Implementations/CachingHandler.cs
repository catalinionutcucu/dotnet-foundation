using Dotnet.Foundation.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Dotnet.Foundation.Implementations;

public sealed class CachingHandler : ICachingHandler
{
    private readonly IDistributedCache _distributedCache;

    public CachingHandler(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    /// <inheritdoc />
    public async Task<TValue?> GetAsync<TValue>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var valueBytes = await _distributedCache.GetAsync(key, cancellationToken)
                                                .ConfigureAwait(false);

        var value = valueBytes is null ? default : JsonSerializer.Deserialize<TValue>(valueBytes);

        return value;
    }

    /// <inheritdoc />
    public async Task SetAsync<TValue>(string key, TValue value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        var valueBytes = JsonSerializer.SerializeToUtf8Bytes(value);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };

        await _distributedCache.SetAsync(key, valueBytes, options, cancellationToken)
                               .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        await _distributedCache.RemoveAsync(key, cancellationToken)
                               .ConfigureAwait(false);
    }
}

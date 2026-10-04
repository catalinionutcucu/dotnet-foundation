using Dotnet.Foundation.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Dotnet.Foundation.Implementations;

/// <summary>
/// Represents the cache implementing <see cref = "ICachingHandler" />, storing values serialized as JSON in an <see cref = "IDistributedCache" />.
/// </summary>
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
    public async Task<TValue> GetOrSetAsync<TValue>(string key, Func<CancellationToken, Task<TValue>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        var valueBytes = await _distributedCache.GetAsync(key, cancellationToken)
                                                .ConfigureAwait(false);

        if (valueBytes is not null)
        {
            return JsonSerializer.Deserialize<TValue>(valueBytes)!;
        }

        var value = await factory(cancellationToken).ConfigureAwait(false);

        if (value is not null)
        {
            await SetAsync(key, value, expiration, cancellationToken).ConfigureAwait(false);
        }

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

using System.Text.Json;

using StackExchange.Redis;

namespace Redis.Api.Services.Caching;

/// <summary>
/// An implementation of <see cref="ICachingService" /> using Redis directly.
/// </summary>
public class RedisCachingService : ICachingService
{
    private readonly ILogger<RedisCachingService> _logger;

    private readonly IConnectionMultiplexer _redis;

    public RedisCachingService(
        ILogger<RedisCachingService> logger,
        IConnectionMultiplexer redis)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T? value, TimeSpan expiry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var db = _redis.GetDatabase();

        _logger.LogInformation("Storing value for key '{Key}' into cache", key);
        await db.StringSetAsync(
            key: key,
            value: JsonSerializer.Serialize(value),
            expiry: expiry).ConfigureAwait(false);
        _logger.LogInformation("Stored value for key '{Key}' into cache", key);
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var db = _redis.GetDatabase();

        _logger.LogInformation("Looking for a cached value for key '{Key}'", key);

        if (await db.KeyExistsAsync(key).ConfigureAwait(false) == false)
        {
            _logger.LogInformation("No cached value found for key '{Key}'", key);
            return default(T);
        }

        var value = await db.StringGetAsync(key).ConfigureAwait(false);

        _logger.LogInformation("Found cached value for key '{Key}'", key);
        return JsonSerializer.Deserialize<T>(value!);
    }

    /// <inheritdoc />
    public async Task InvalidateAsync(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var db = _redis.GetDatabase();

        _logger.LogInformation("Invalidating the cached value for key '{Key}'", key);
        await db.KeyDeleteAsync(key).ConfigureAwait(false);
        _logger.LogInformation("Invalidated the cached value for key '{Key}'", key);
    }
}
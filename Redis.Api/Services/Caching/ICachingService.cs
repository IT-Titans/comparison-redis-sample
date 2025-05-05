namespace Redis.Api.Services.Caching;

/// <summary>
/// Abstraction to show different implementations between using redis directly vs. using redis via the IDistributedCache.
/// </summary>
public interface ICachingService
{
    /// <summary>
    /// Sets a value associated with a key into the cache.
    /// </summary>
    /// <param name="key">The key the value is associated with.</param>
    /// <param name="value">The value that needs to be cached.</param>
    /// <param name="expiry">When the cached value will expire relative to now.</param>
    Task SetAsync<T>(string key, T? value, TimeSpan expiry);

    /// <summary>
    /// Gets a value from cache for the given key.
    /// </summary>
    /// <param name="key">The key the value is associated with.</param>
    /// <returns>The value retrieved from cache; otherwise default.</returns>
    Task<T?> GetAsync<T>(string key);
    
    /// <summary>
    /// Invalidates the value for the given key.
    /// </summary>
    /// <param name="key">The key the value is associated with.</param>
    Task InvalidateAsync(string key);
}
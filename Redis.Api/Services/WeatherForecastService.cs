using Redis.Api.Models;
using Redis.Api.Services.Caching;

namespace Redis.Api.Services;

public class WeatherForecastService
{
    private readonly ILogger<WeatherForecastService> _logger;

    private readonly ICachingService _cachingService;

    public WeatherForecastService(
        ILogger<WeatherForecastService> logger,
        ICachingService cachingService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cachingService = cachingService ?? throw new ArgumentNullException(nameof(cachingService));
    }

    /// <summary>
    /// Gets a weather forecast for the given <paramref name="city" />.
    /// </summary>
    /// <param name="city">The name of the city to get a weather forecast for.</param>
    /// <returns>The forecast.</returns>
    public async Task<WeatherForecast> GetWeatherForecastAsync(string city)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(city);

        var key = $"weather:{city}";

        _logger.LogInformation("Reading weather forecast for city '{City}' from cache", city);
        var cachedForecast = await _cachingService.GetAsync<WeatherForecast>(key).ConfigureAwait(false);
        if (cachedForecast is null)
        {
            // The weather forecast for the given key is not cached yet, so we need to compute it
            // Simulate performance intensive work by computing a weather forecast
            var computedForecast = await ComputeWeatherForecastAsync(city).ConfigureAwait(false);

            await _cachingService.SetAsync(
                key: key,
                value: computedForecast,
                expiry: TimeSpan.FromMinutes(1) // The forecast is only valid for 1 minute. After that, it needs to be recreated.
            ).ConfigureAwait(false);

            return computedForecast;
        }
        else
        {
            return cachedForecast;
        }
    }

    /// <summary>
    /// Invalidates a weather forecast for the given <paramref name="city" />.
    /// </summary>
    /// <param name="city">The name of the city to invalidate / recreate a weather forecast for.</param>
    public async Task InvalidateForecastAsync(string city)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(city);

        var key = $"weather:{city}";

        _logger.LogInformation("Invalidating weather forecast for city '{City}' forcing it to be recreated", city);
        await _cachingService.InvalidateAsync(key).ConfigureAwait(false);
    }

    /// <summary>
    /// Simulates compute-heavy operation to get a weather forecast.
    /// </summary>
    /// <returns>The computed weather forecast</returns>
    private async Task<WeatherForecast> ComputeWeatherForecastAsync(string city)
    {
        _logger.LogInformation("Computing weather forecast for city '{City}'", city);

        await Task.Delay(TimeSpan.FromSeconds(3)).ConfigureAwait(false);

        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        var weatherForecasts = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();

        _logger.LogInformation("Computed weather forecast for city '{City}'", city);

        return weatherForecasts[Random.Shared.Next(weatherForecasts.Length)];
    }
}
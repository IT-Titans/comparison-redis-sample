namespace Redis.Api.Models;

/// <summary>
/// Represents a weather forecast.
/// </summary>
/// <param name="Date">The forecasting date.</param>
/// <param name="TemperatureC">Forecasted temperature in °C.</param>
/// <param name="Summary">Description of the forecasted weather.</param>
public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

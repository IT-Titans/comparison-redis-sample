using Microsoft.AspNetCore.Mvc;

using Redis.Api.Services;
using Redis.Api.Services.Caching;

using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

const string redisConfiguration = "localhost:6379";

var cacheImplementation = args.Length == 1 
    ? args[0] 
    : "direct";

RegisterCachingServices(builder.Services, cacheImplementation, redisConfiguration);

builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Retrieves a weather forecast for the given city.
app.MapGet("/weatherforecast/{city}", async (
        [FromRoute] string city,
        [FromServices] WeatherForecastService forecastService) =>
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return Results.BadRequest("Please provide a valid city.");
        }

        return Results.Ok(await forecastService.GetWeatherForecastAsync(city).ConfigureAwait(false));
    })
    .WithName("GetWeatherForecastForCity");

// Invalidates a cached weather forecast for a city.
app.MapGet("/invalidate/{city}", async (
        [FromRoute] string city,
        [FromServices] WeatherForecastService forecastService) =>
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return Results.BadRequest("Please provide a valid city.");
        }

        await forecastService.InvalidateForecastAsync(city).ConfigureAwait(false);

        return Results.Ok();
    })
    .WithName("InvalidateWeatherForecastForCity");

app.Run();

void RegisterCachingServices(IServiceCollection services, string mode, string configuration)
{
    switch (mode)
    {
        case "direct":
            services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(configuration));
            services.AddSingleton<ICachingService, RedisCachingService>();
            break;

        case "distributed":
            services.AddStackExchangeRedisCache(options => options.Configuration = configuration);
            services.AddSingleton<ICachingService, RedisDistributedCachingService>();
            break;

        default:
            throw new ArgumentException($"Unsupported cache mode: '{mode}'. Valid options are 'direct' or 'distributed'.");
    }
}